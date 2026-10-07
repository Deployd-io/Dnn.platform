import { Component, Host, h, Element, Event, EventEmitter, State } from '@stencil/core';
import { ItemsClient } from '../../services/ItemsClient';
import state from '../../store/store';
const logger = console;

@Component({
  tag: 'dnn-rm-upload-file',
  styleUrl: 'dnn-rm-upload-file.scss',
  shadow: true,
})
export class DnnRmUploadFile {

  /**
   * Fires when there is a possibility that some folders have changed.
   * Can be used to force parts of the UI to refresh.
   */
  @Event() dnnRmFoldersChanged: EventEmitter<void>;

  @State() queuedFiles : File[] = [];
  @State() alreadyExistingFilesCount = 0;
  @State() uploadingFilesCount = 0;
  @State() uploadedFilesCount = 0;
  @State() failedFilesCount = 0;

  @Element() el: HTMLDnnRmUploadFileElement;

  private itemsClient: ItemsClient;
  private allowedExtensions: string;
  private validationCode: string;
  private maxUploadFileSize: number;
  private extract = false;

  constructor() {
	logger.error('>>> Entering constructor()', { "event.action": "constructor" });
    this.itemsClient = new ItemsClient(state.moduleId);
	logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
  }

  async componentWillLoad() {
	logger.error('>>> Entering componentWillLoad()', { "event.action": "componentWillLoad" });
    try {
      const data = await this.itemsClient.getAllowedFileExtensions();
	logger.error(`componentWillLoad(): data → ${data}`, { "event.action": "componentWillLoad" });
      this.allowedExtensions = data.allowedExtensions;
      this.validationCode = data.validationCode;
      this.maxUploadFileSize = data.maxUploadFileSize;
    } catch (error) {
      alert(error);
	logger.warn('<<< Exiting componentWillLoad()', { "event.action": "componentWillLoad" });
    }
  }

  private handleFilesDropped(droppedFiles: File[]): void {
	logger.error(`>>> Entering handleFilesDropped(droppedFiles=${droppedFiles})`, { "event.action": "handleFilesDropped", "logrite.droppedFiles": droppedFiles });
    droppedFiles.forEach(file =>{
      this.queuedFiles = [...this.queuedFiles, file];
    });
	logger.warn(`<<< Exiting handleFilesDropped(droppedFiles=${droppedFiles})`, { "event.action": "handleFilesDropped", "logrite.droppedFiles": droppedFiles });
  }
  
  render() {
	logger.error('>>> Entering render()', { "event.action": "render" });
	logger.warn('<<< Exiting render()', { "event.action": "render" });
    return (
      <Host>
        <h2>{state.localization.Upload}</h2>
        <label>
          <dnn-checkbox onClick={() => this.extract = !this.extract} />
          {state.localization.ExtractUploads}
        </label>
        <dnn-dropzone
          onFilesSelected={e => this.handleFilesDropped(e.detail)}
          multiple
        />
        {this.allowedExtensions && this.validationCode && this.queuedFiles.map(file => 
          <dnn-rm-queued-file
            file={file}
            validationCode={this.validationCode}
            filter={this.allowedExtensions}
            extract={this.extract}
            maxUploadFileSize={this.maxUploadFileSize}
          />
        )}
      </Host>
    );
  }
}
