const logger = console;
import { IInitStylesConfig, INotifyOptions } from "../window.dnn";

export default class StylesClient {
    private serviceRoot: string;
    private headers: Headers;
    private config: IInitStylesConfig;
    private isHost: boolean = false;

    constructor() {
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        const dnnStyles = window.dnn as unknown as IDnnWrapper;
        this.config = dnnStyles.initStyles();
        if (this.config == undefined) {
            throw new Error("dnn.initStyles() is not defined.");
        }
        if (this.config.utility == undefined) {
            throw new Error("dnn.initStyles().utility is not defined.");
        }

        const sf = this.config.utility.sf;
        sf.moduleRoot = "PersonaBar";
		logger.debug(`constructor(): sf.moduleRoot → ${sf.moduleRoot}`, { "event.action": "constructor" });
        this.serviceRoot = sf.getServiceRoot();
        this.serviceRoot += "Styles/";

        const headers = new Headers();
		logger.debug(`constructor(): headers → ${headers}`, { "event.action": "constructor" });
        headers.append("RequestVerificationToken", sf.antiForgeryToken);
        headers.append("Content-Type", "application/json");

        this.headers = headers;

		logger.debug(`constructor(): this.headers → ${this.headers}`, { "event.action": "constructor" });
        this.isHost = this.config?.params?.settings?.isHost ?? false;
		logger.debug(`constructor(): this.isHost → ${this.isHost}`, { "event.action": "constructor" });
    }

    public get isHostUser(): boolean {
        return this.isHost;
    }

    getStyles(){
		logger.error('>>> Entering getStyles()', { "event.action": "getStyles" });
		logger.warn('<<< Exiting getStyles()', { "event.action": "getStyles" });
        return new Promise<IPortalStyles>((resolve, reject) => {
            fetch(`${this.serviceRoot}GetStyles`, {
                method: "GET",
            })
            .then((response) => {
                if (!response.ok) {
					const _start_getStyles_37 = Date.now();
                    throw new Error(response.statusText);
                }
                return response.json();
            })
            .then((response) => {
                resolve(response as IPortalStyles);
            })
            .catch(error => {
                reject(Error(error as string));
            });
        });
    }

    saveStyles(styles: IPortalStyles){
		logger.error(`>>> Entering saveStyles(styles=${styles})`, { "event.action": "saveStyles", "logrite.styles": styles });
		logger.warn(`<<< Exiting saveStyles(styles=${styles})`, { "event.action": "saveStyles", "logrite.styles": styles });
        return new Promise<void>((resolve, reject) => {
            fetch(`${this.serviceRoot}SaveStyles`, {
                method: "POST",
                headers: this.headers,
                body: JSON.stringify(styles),
            })
            .then(response => {
                if (!response.ok) {
					const _start_saveStyles_57 = Date.now();
                    throw new Error(response.statusText);
                }
                resolve();
            })
            .catch((error) => {
                reject(new Error(error as string));
            });
        });
    }

    restoreStyles(){
		logger.error('>>> Entering restoreStyles()', { "event.action": "restoreStyles" });
		logger.warn('<<< Exiting restoreStyles()', { "event.action": "restoreStyles" });
        return new Promise<IPortalStyles>((resolve, reject) => {
            fetch(`${this.serviceRoot}RestoreStyles`, {
                method: "POST",
                headers: this.headers,
            })
            .then(response => {
                if (!response.ok) {
					const _start_restoreStyles_76 = Date.now();
                    throw new Error(response.statusText);
                }
                return response.json();
            })
            .then((response) => {
                resolve(response as IPortalStyles);
            })
            .catch((error) => {
                reject(Error(error as string));
            });
        });
    }

    notify(message: string, options?: INotifyOptions){
		logger.error(`>>> Entering notify(message=${message},options=${options})`, { "event.action": "notify", "logrite.message": message, "logrite.options": options });
        this.config.utility.notify(message, options);
		logger.warn(`<<< Exiting notify(message=${message},options=${options})`, { "event.action": "notify", "logrite.message": message, "logrite.options": options });
    }

    notifyError(message: string, options?: INotifyOptions){
		logger.error(`>>> Entering notifyError(message=${message},options=${options})`, { "event.action": "notifyError", "logrite.message": message, "logrite.options": options });
        if (options == undefined) {
            options = {
                clickToClose: true,
            };
        }

        if (options.clickToClose == undefined) {
            options.clickToClose = true;
			logger.debug(`notifyError(message=${message},options=${options}): options.clickToClose → ${options.clickToClose}`, { "event.action": "notifyError", "logrite.message": message, "logrite.options": options });
        }

        this.config.utility.notifyError(message, options);
		logger.warn(`<<< Exiting notifyError(message=${message},options=${options})`, { "event.action": "notifyError", "logrite.message": message, "logrite.options": options });
    }
}
export interface IPortalStyles {
    AllowAdminEdits: boolean;
    ColorPrimary: string;
    ColorPrimaryLight: string;
    ColorPrimaryDark: string;
    ColorPrimaryContrast: string;
    ColorSecondary: string;
    ColorSecondaryLight: string;
    ColorSecondaryDark: string;
    ColorSecondaryContrast: string;
    ColorTertiary: string;
    ColorTertiaryLight: string;
    ColorTertiaryDark: string;
    ColorTertiaryContrast: string;
    ColorNeutral: string;
    ColorNeutralLight: string;
    ColorNeutralDark: string;
    ColorNeutralContrast: string;
    ColorBackground: string;
    ColorBackgroundLight: string;
    ColorBackgroundDark: string;
    ColorBackgroundContrast: string;
    ColorForeground: string;
    ColorForegroundLight: string;
    ColorForegroundDark: string;
    ColorForegroundContrast: string;
    ColorSurface: string;
    ColorSurfaceLight: string;
    ColorSurfaceDark: string;
    ColorSurfaceContrast: string;
    ColorInfo: string;
    ColorInfoLight: string;
    ColorInfoDark: string;
    ColorInfoContrast: string;
    ColorSuccess: string;
    ColorSuccessLight: string;
    ColorSuccessDark: string;
    ColorSuccessContrast: string;
    ColorWarning: string;
    ColorWarningLight: string;
    ColorWarningDark: string;
    ColorWarningContrast: string;
    ColorDanger: string;
    ColorDangerLight: string;
    ColorDangerDark: string;
    ColorDangerContrast: string;
    ControlsRadius: number;
    ControlsPadding: number;
    BaseFontSize: number;
    VariationOpacity: number;
}