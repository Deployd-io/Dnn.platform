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
	logger.debug('>>> Entering constructor()');
    this.itemsClient = new ItemsClient(state.moduleId);
	logger.debug('<<< Exiting constructor()');
  }

  async componentWillLoad() {
	logger.debug('>>> Entering componentWillLoad()');
    try {
      const data = await this.itemsClient.getAllowedFileExtensions();
	logger.debug(`componentWillLoad(): data → ${data}`);
      this.allowedExtensions = data.allowedExtensions;
      this.validationCode = data.validationCode;
      this.maxUploadFileSize = data.maxUploadFileSize;
    } catch (error) {
      alert(error);
	logger.debug('<<< Exiting componentWillLoad()');
    }
  }

  private handleFilesDropped(droppedFiles: File[]): void {
	logger.debug(`>>> Entering handleFilesDropped(droppedFiles=${droppedFiles})`);
    droppedFiles.forEach(file =>{
      this.queuedFiles = [...this.queuedFiles, file];
    });
	logger.debug(`<<< Exiting handleFilesDropped(droppedFiles=${droppedFiles})`);
  }
  
  render() {
	logger.debug('>>> Entering render()');
	logger.debug('<<< Exiting render()');
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
