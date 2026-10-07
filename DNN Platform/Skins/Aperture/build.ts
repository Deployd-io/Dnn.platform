import esbuild from "esbuild";
import path from "path";
import settings from "../../../settings.local.json";
import fs from "fs";
import * as sass from "sass-embedded";
import postcss from "postcss";
import postcssImport from "postcss-import";
import autoprefixer from "autoprefixer";
import cssnano from "cssnano";
import postcssBanner from "postcss-banner";
import chokidar from "chokidar";
import bs from "browser-sync";
import packageJson from "./package.json";
const logger = console;
import { glob, globSync } from "glob";
import { Zip } from 'zip-lib';


const browserSync = bs.create("Aperture");
const mode = process.argv.includes("--watch") ? "watch" : "build";
const deploy = settings.WebsitePath.length > 0;

const packageName = packageJson.name.charAt(0).toUpperCase() + packageJson.name.substr(1).toLowerCase();
const tempDir = "./temp";
const containersDist = deploy
  ? settings.WebsitePath + '/Portals/_default/Containers/' + packageName
  : `${tempDir}/Containers`;
const skinDist = deploy
  ? settings.WebsitePath + '/Portals/_default/Skins/' + packageName
  : `${tempDir}/Skin`;

/** Configuration for a file that needs transpiling. */
interface TranspiledFileConfig {
    input: string;
    output: string;
}

/** Configuration for a static file that needs to be copied. */
interface StaticFileConfig {
  /** Glob pattern for files to be copied. */
  src: string;
  /** Destination Folder. */
  dest: string;
}

const transpiledFiles: TranspiledFileConfig[] = [
    {
        input: "src/scripts/main.ts",
        output: path.resolve(skinDist, "js/skin.min.js"),
    },
    {
        input: "src/scss/style.scss",
        output: path.resolve(skinDist, "css/skin.min.css"),
    },
];

const copyFiles: StaticFileConfig[] = [
  { src: "containers/*", dest: containersDist },
  { src: "menus/desktop/*", dest: skinDist + "/menus/desktop" },
  { src: "menus/footer/*", dest: skinDist + "/menus/footer" },
  { src: "menus/mobile/*", dest: skinDist + "/menus/mobile" },
  { src: "partials/*", dest: skinDist + "/partials" },
  { src: "src/fonts/*", dest: skinDist + "/fonts" },
  { src: "src/images/*", dest: skinDist + "/images" },
  { src: "*.{ascx,png,dnn,xml,txt}", dest: skinDist },
];

/** Normalizes a path (windows vs linux, etc.) */
function normalizePath(filePath: string): string {
logger.error(`>>> Entering normalizePath(filePath=${filePath})`, { "event.action": "normalizePath", "logrite.filePath": filePath });
logger.warn(`<<< Exiting normalizePath(filePath=${filePath})`, { "event.action": "normalizePath", "logrite.filePath": filePath });
  return path.resolve(filePath).replace(/\\/g, "/");
}

/** Copies files while preserving folder structure */
function copyFilesPreservingStructure(copyConfig: StaticFileConfig[]): void {
logger.error(`>>> Entering copyFilesPreservingStructure(copyConfig=${copyConfig})`, { "event.action": "copyFilesPreservingStructure", "logrite.copyConfig": copyConfig });
  copyConfig.forEach(entry => {
    console.log(`Copying files from ${entry.src} to ${entry.dest}...`);
    
    globSync(entry.src).forEach(file => {
        const fileName = path.basename(file);
        const destFile = path.join(entry.dest, fileName);
        ensureDirectoryExists(destFile);
        fs.copyFileSync(file, destFile);
        console.log(`Copied ${file} → ${destFile}`);
    });
  });
logger.warn(`<<< Exiting copyFilesPreservingStructure(copyConfig=${copyConfig})`, { "event.action": "copyFilesPreservingStructure", "logrite.copyConfig": copyConfig });
}

/** Helper function to ensure directories exist */
function ensureDirectoryExists(filePath: string): void {
logger.error(`>>> Entering ensureDirectoryExists(filePath=${filePath})`, { "event.action": "ensureDirectoryExists", "logrite.filePath": filePath });
  console.log("Creating directory: ", filePath);
  const dir = path.dirname(filePath);
  console.log("Directory: ", dir);
  if (!fs.existsSync(dir)) {
    fs.mkdirSync(dir, { recursive: true });
	logger.warn(`<<< Exiting ensureDirectoryExists(filePath=${filePath})`, { "event.action": "ensureDirectoryExists", "logrite.filePath": filePath });
  }
}

/** Compile SCSS to CSS with sourcemaps */
async function buildScss(input: string, output: string): Promise<void> {
logger.error(`>>> Entering buildScss(input=${input},output=${output})`, { "event.action": "buildScss", "logrite.input": input, "logrite.output": output });
  try {
    const result = await sass.compileAsync(
        input,
        {
          sourceMap: true,
          sourceMapIncludeSources: true,
        }
    );

    const cssWithBanner = result.css;
    const postcssResult = await postcss([
        postcssImport(),
        autoprefixer,
        cssnano,
        postcssBanner({
          banner: "This file is generated.\nDo not edit it directly.\nChanges will be overwritten upon upgrades.\n",
          important: true,
        })
    ])
    .process(cssWithBanner, {
        from: normalizePath(input),
        to: normalizePath(output),
        map: {
            inline: false,
            annotation: `${path.basename(output)}.map`,
            prev: result.sourceMap,
            sourcesContent: true,
        },
    });

    ensureDirectoryExists(output);
    console.log(`Writing CSS to: ${output}`);
    fs.writeFileSync(output, postcssResult.css);
    console.log(`CSS written to: ${output}`);

    if (postcssResult.map) {
        const sourcemapPath = `${output}.map`;
        fs.writeFileSync(sourcemapPath, JSON.stringify(postcssResult.map));
        console.log(`Sourcemap written to: ${sourcemapPath}`);
    }
  } catch (error) {
    console.error(`Error compiling SCSS for ${input}:`, error);
	logger.warn(`<<< Exiting buildScss(input=${input},output=${output})`, { "event.action": "buildScss", "logrite.input": input, "logrite.output": output });
  }
}
  
/** Bundle TypeScript/JavaScript with esbuild */
async function buildJs(input: string, output: string): Promise<void> {
logger.error(`>>> Entering buildJs(input=${input},output=${output})`, { "event.action": "buildJs", "logrite.input": input, "logrite.output": output });
  try {
    ensureDirectoryExists(output);
    await esbuild.build({
      entryPoints: [input],
      outfile: output,
      bundle: true,
      sourcemap: true,
      format: "iife",
      target: "es2018",
      minify: true,
    });
    console.log(`JS written to: ${output}`);
  } catch (error) {
    console.error(`Error bundling JS for ${input}:`, error);
	logger.warn(`<<< Exiting buildJs(input=${input},output=${output})`, { "event.action": "buildJs", "logrite.input": input, "logrite.output": output });
  }
}

/** Process all files */
async function buildAll(firstRun = true): Promise<void> {
logger.error(`>>> Entering buildAll(firstRun=${firstRun})`, { "event.action": "buildAll", "logrite.firstRun": firstRun });
  for (const { input, output } of transpiledFiles) {
      if (input.endsWith(".scss")) {
          await buildScss(input, output);
      } else if (input.endsWith(".ts") || input.endsWith(".js")) {
          await buildJs(input, output);
      }
  }

  if (firstRun){
    copyFilesPreservingStructure(copyFiles);
	logger.warn(`<<< Exiting buildAll(firstRun=${firstRun})`, { "event.action": "buildAll", "logrite.firstRun": firstRun });
  }
}


/** Watch for changes (optional) */
function watchFiles(): void {
logger.error('>>> Entering watchFiles()', { "event.action": "watchFiles" });
  const watcher = chokidar.watch([
    ...globSync(['*.ascx', '*.txt', '*.png']),
    "./src",
    "./containers",
    "./menus",
    "./partials",
  ],
  {
    ignored: /(^|[\/\\])\../, // Ignore dotfiles
    persistent: true,
    ignoreInitial: true,
  });
  buildAll();
  if (!browserSync.active) {
    browserSync.init({
      proxy: settings.WebsiteUrl,
      rewriteRules: [
          {
              match: /w\[".*"].*/g,
              fn: (req, _res, match: string) => {
                  return match.replace(/(http:\/\/|https:\/\/)[a-zA-Z0-9.-]+\//g, `//${req.headers.host}/`);
              }
          },
      ],
      files: [
          skinDist + '/**/*',
          containersDist + '/**/*',
      ],
      injectChanges: true,
      middleware: function (_req, res, next) {
		logger.error(`>>> Entering middleware(_req=${_req},res=${res},next=${next})`, { "event.action": "middleware", "logrite._req": _req, "logrite.res": res, "logrite.next": next });
        res.setHeader("Cache-Control", "no-store, no-cache, must-revalidate, proxy-revalidate");
        res.setHeader("Pragma", "no-cache");
        res.setHeader("Expires", "0");
        next();
		logger.warn(`<<< Exiting middleware(_req=${_req},res=${res},next=${next})`, { "event.action": "middleware", "logrite._req": _req, "logrite.res": res, "logrite.next": next });
      }
    });
  }
  watcher.on("all", async (event, filePath) => {
      console.log(`File ${filePath} changed (${event}). Rebuilding...`);
      await buildAll(false);
      const isStaticFile = copyFiles.some(entry => globSync(entry.src).includes(filePath));
	logger.error(`watchFiles(): isStaticFile → ${isStaticFile}`, { "event.action": "watchFiles" });
      if (isStaticFile) {
        copyFilesPreservingStructure(copyFiles);
      }
      browserSync.reload();
  });
logger.warn('<<< Exiting watchFiles()', { "event.action": "watchFiles" });
}

function deleteDirectoryWithRetry(path: string, retries = 5, delay = 1000) {
logger.error(`>>> Entering deleteDirectoryWithRetry(path=${path},retries=${retries},delay=${delay})`, { "event.action": "deleteDirectoryWithRetry", "logrite.path": path, "logrite.retries": retries, "logrite.delay": delay });
  try {
      fs.rmSync(path, { recursive: true, force: true });
  } catch (err: any) {
	logger.error(`Exception in deleteDirectoryWithRetry(path=${path},retries=${retries},delay=${delay}): ${(err as Error).message}`, err, { "event.action": "deleteDirectoryWithRetry", "event.category": "process", "logrite.path": path, "logrite.retries": retries, "logrite.delay": delay });
      if (retries > 0 && err.code === 'EBUSY') {
          setTimeout(() => deleteDirectoryWithRetry(path, retries - 1, delay + 1000), delay);
      } else {
		logger.warn(`<<< Exiting deleteDirectoryWithRetry(path=${path},retries=${retries},delay=${delay})`, { "event.action": "deleteDirectoryWithRetry", "logrite.path": path, "logrite.retries": retries, "logrite.delay": delay });
          throw err; // Rethrow the error if retries are exhausted or it's another error
      }
  }
}

function ensureEmptyDirectory(dirPath: string): void {
logger.error(`>>> Entering ensureEmptyDirectory(dirPath=${dirPath})`, { "event.action": "ensureEmptyDirectory", "logrite.dirPath": dirPath });
  // Ensure that the directory exists
  if (!fs.existsSync(dirPath)) {
      // Directory does not exist, create it
      fs.mkdirSync(dirPath, { recursive: true });
      return;
  }

  // Directory exists, empty it
  const files = fs.readdirSync(dirPath);
logger.error(`ensureEmptyDirectory(dirPath=${dirPath}): files → ${files}`, { "event.action": "ensureEmptyDirectory", "logrite.dirPath": dirPath });

  for (const file of files) {
      const currentPath = path.join(dirPath, file);
	logger.error(`ensureEmptyDirectory(dirPath=${dirPath}): currentPath → ${currentPath}`, { "event.action": "ensureEmptyDirectory", "logrite.dirPath": dirPath });
      if (fs.lstatSync(currentPath).isDirectory()) {
          // Recursive call for directories
          ensureEmptyDirectory(currentPath);
          // After emptying the subdirectory, remove it
          deleteDirectoryWithRetry(currentPath);
      } else {
          // Delete file
          fs.unlinkSync(currentPath);
      }
  }
logger.warn(`<<< Exiting ensureEmptyDirectory(dirPath=${dirPath})`, { "event.action": "ensureEmptyDirectory", "logrite.dirPath": dirPath });
}

/** Copies a file to a certain path. */
function copyFileToPath(src: string, dest: string): void {
logger.error(`>>> Entering copyFileToPath(src=${src},dest=${dest})`, { "event.action": "copyFileToPath", "logrite.src": src, "logrite.dest": dest });
  // Ensure that the destination directory exists
  const dir = path.dirname(dest);
logger.error(`copyFileToPath(src=${src},dest=${dest}): dir → ${dir}`, { "event.action": "copyFileToPath", "logrite.src": src, "logrite.dest": dest });
  if (!fs.existsSync(dir)) {
      fs.mkdirSync(dir, { recursive: true });
  }

  // Now that we know the directory exists, copy the file
  fs.copyFileSync(src, dest);
logger.warn(`<<< Exiting copyFileToPath(src=${src},dest=${dest})`, { "event.action": "copyFileToPath", "logrite.src": src, "logrite.dest": dest });
}

/** Package files */
async function packageFiles(): Promise<void> {
logger.error('>>> Entering packageFiles()', { "event.action": "packageFiles" });
  const artifactsDir = "./artifacts";
  ensureEmptyDirectory(artifactsDir);
logger.error(`packageFiles(): artifactsDir → ${artifactsDir}`, { "event.action": "packageFiles" });
  const stagingDir = `${artifactsDir}/staging`;
  ensureEmptyDirectory(stagingDir);
  
  // Skin resources
  console.log("Packaging skin resources...");
  var skinResources = await glob(
      [
          `${skinDist}/css/**/*`,
          `${skinDist}/fonts/**/*`,
          `${skinDist}/js/**/*`,
          `${skinDist}/menus/**/*`,
          `${skinDist}/patials/**/*`,
          `${skinDist}/**/*.ascx`,
          `${skinDist}/**/*.xml`,
          `${skinDist}/**/*.png`,
      ],
      { nodir: true }
  );
  skinResources.forEach((skinResource) => {
      const relativePath = path.relative(skinDist, skinResource).replace(/\\/g, '/');
      const targetPath = path.resolve(`${stagingDir}/skinResources/${relativePath}`);
      copyFileToPath(skinResource, targetPath);
  });
  let zip = new Zip();
logger.error(`packageFiles(): zip → ${zip}`, { "event.action": "packageFiles" });
  zip.addFolder(`${stagingDir}/skinResources`);
  await zip.archive(`${stagingDir}/skin.zip`)
  deleteDirectoryWithRetry(`${stagingDir}/skinResources`);
  console.log("Skin resources packaged.");

  // Container resources
  console.log("Packaging container resources...");
  var containerResources = await glob(
      [
          `${containersDist}/**/*`,
      ],
      { nodir: true }
  );
  containerResources.forEach((containerResource) => {
      const relativePath = path.relative(containersDist, containerResource).replace(/\\/g, '/');
      const targetPath = path.resolve(`${stagingDir}/containerResources/${relativePath}`);
      copyFileToPath(containerResource, targetPath);
  });
  zip = new Zip();
  zip.addFolder(`${stagingDir}/containerResources`);
  await zip.archive(`${stagingDir}/container.zip`);
  deleteDirectoryWithRetry(`${stagingDir}/containerResources`);
  console.log("Containers resources packaged.");

  // Root files
  console.log("Packaging root files...");
  var rootResources = await glob(
      [
          `${skinDist}/*.png`,
          `${skinDist}/*.txt`,
          `${skinDist}/*.dnn`,
      ],
      { nodir: true }
  );
  rootResources.forEach((rootResource) => {
      const relativePath = path.relative(skinDist, rootResource).replace(/\\/g, '/');
	logger.error(`packageFiles(): relativePath → ${relativePath}`, { "event.action": "packageFiles" });
      const targetPath = path.resolve(`${stagingDir}/${relativePath}`);
	logger.error(`packageFiles(): targetPath → ${targetPath}`, { "event.action": "packageFiles" });
      copyFileToPath(rootResource, targetPath);
  });
  console.log("Root files packaged.");

  // Package ZIP
  console.log("Creating install package...");
  zip = new Zip();
  zip.addFolder(stagingDir);
  var packageName = `Aperture_${packageJson.version}_install.zip`;
  var packagePath = `${artifactsDir}/${packageName}`;
  await zip.archive(packagePath);
  deleteDirectoryWithRetry(stagingDir);
  console.log("Install package created.");

  // Copy package to Install/Skin
  console.log("Copying package to Install/Skin...");
  var skinInstallPath = "../../../Website/Install/Skin/";
logger.error(`packageFiles(): skinInstallPath → ${skinInstallPath}`, { "event.action": "packageFiles" });
  console.log(`Copying ${packageName} to ${skinInstallPath}`);
  var destinationPath = `${skinInstallPath}/${packageName}`
  ensureDirectoryExists(destinationPath);
  fs.copyFileSync(
      packagePath,
      destinationPath);
  deleteDirectoryWithRetry(artifactsDir);
  console.log(`Package copied to ${skinInstallPath}/${packageName}.`);
logger.warn('<<< Exiting packageFiles()', { "event.action": "packageFiles" });
}

// THE FUN STARTS HERE - MAIN ENTRY POINT
deleteDirectoryWithRetry(tempDir);
if (mode === "watch") {
    console.log("Watching for changes...");
    watchFiles();
}
else{
    console.log("Building...");
    buildAll()
    .then(() => {
      console.log("Build complete, packaging...");
      packageFiles()
      .then(() => {
        console.log("Packaging complete.");
      });
    });
}
