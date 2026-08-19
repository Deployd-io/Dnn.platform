import { Component, Host, h, Element, State, Event, EventEmitter } from '@stencil/core';
import { CreateNewFolderRequest, FolderMappingInfo, ItemsClient } from '../../services/ItemsClient';
import state from '../../store/store';
const logger = console;

@Component({
  tag: 'dnn-rm-create-folder',
  styleUrl: 'dnn-rm-create-folder.scss',
  shadow: true,
})
export class DnnRmCreateFolder {

  /**
   * Fires when there is a possibility that some folders have changed.
   * Can be used to force parts of the UI to refresh.
   */
  @Event() dnnRmFoldersChanged: EventEmitter<void>;
  
  @Element() el : HTMLDnnRmCreateFolderElement
  
  @State() folderMappings: FolderMappingInfo[];
  
  @State() newFolderRequest: CreateNewFolderRequest = {
    FolderMappingId: -1,
    FolderName: "",
    ParentFolderId: state.currentItems.folder.folderId,
  }

  private readonly itemsClient: ItemsClient;
  private nameField: HTMLDnnInputElement;

  constructor(){
	logger.debug('>>> Entering constructor()');
    this.itemsClient = new ItemsClient(state.moduleId);
	logger.debug('<<< Exiting constructor()');
  }

  async componentWillLoad() {
	logger.debug('>>> Entering componentWillLoad()');
    try {
      const data = await this.itemsClient.getFolderMappings();
	logger.debug(`componentWillLoad(): data → ${data}`);
      this.folderMappings = data.sort((a, b) => a.FolderMappingID - b.FolderMappingID);
      this.newFolderRequest = {
        ...this.newFolderRequest,
        FolderMappingId: this.folderMappings[0].FolderMappingID,
        ParentFolderId: state.currentItems.folder.folderId,
      };
    } catch (error) {
      alert(error);
	logger.debug('<<< Exiting componentWillLoad()');
    }
  }

  componentDidLoad() {
	logger.debug('>>> Entering componentDidLoad()');
    setTimeout(() => {
      this.nameField.focus();
    }, 350);
	logger.debug('<<< Exiting componentDidLoad()');
  }

  private async handleCancel() {
	logger.debug('>>> Entering handleCancel()');
    const modal = this.el.parentElement as HTMLDnnModalElement;
    await modal.hide();
    setTimeout(() => {
      document.body.removeChild(modal);
    }, 300);
	logger.debug('<<< Exiting handleCancel()');
  }

  private async handleSave(e: Event) {
	logger.debug(`>>> Entering handleSave(e=${e})`);
    e.preventDefault();
    try {
      await this.itemsClient.createNewFolder(this.newFolderRequest)
        this.dnnRmFoldersChanged.emit();
        state.currentItems = {
          ...state.currentItems,
          items: [],
        };
      const modal = this.el.parentElement as HTMLDnnModalElement;
      await modal.hide();
        setTimeout(() => {
          document.body.removeChild(modal);
        }, 300);
    } catch (error) {
      alert(error);
	logger.debug(`<<< Exiting handleSave(e=${e})`);
    }
  }

  private canChooseFolderProvider() {
	logger.debug('>>> Entering canChooseFolderProvider()');
	logger.debug('<<< Exiting canChooseFolderProvider()');
    return this.folderMappings &&
      this.folderMappings.find(f => f.FolderMappingID == state.currentItems.folder.folderMappingId).IsDefault;
  }

  render() {
	logger.debug('>>> Entering render()');
	logger.debug('<<< Exiting render()');
    return (
      <Host>
        <h2>{state.localization.AddFolder}</h2>
        <form
          onSubmit={e => void this.handleSave(e)}
        >
          <div class="form">
            {state.currentItems.folder.folderName.length > 0 &&[
              <label>{state.localization.FolderParent}</label>,
              <span>{state.currentItems.folder.folderName}</span>
            ]}
            <dnn-input
              type="text"
              label={state.localization.Name}
              required
              ref={el => this.nameField = el}
              value={this.newFolderRequest.FolderName}
              onInput={e => this.newFolderRequest = {
                ...this.newFolderRequest,
                FolderName: (e.target as HTMLInputElement).value,
              }}
            />
            {this.canChooseFolderProvider() &&
              [
                <label>{state.localization.Type}</label>,
                <dnn-select onValueChange={e => this.newFolderRequest = {
                  ...this.newFolderRequest,
                  FolderMappingId: Number.parseInt(e.detail),
                }}>
                  {this.folderMappings?.map((folderMapping, index) => 
                    <option
                      value={folderMapping.FolderMappingID}
                      selected={index == 0}>
                      {folderMapping.MappingName}
                    </option>
                  )}
                </dnn-select>
              ]
            }
            {this.canChooseFolderProvider() && this.folderMappings.find(m => m.FolderMappingID == this.newFolderRequest.FolderMappingId).IsDefault == false &&[
              <dnn-input 
              type="text"
                label={state.localization.MappedPath}
                value={this.newFolderRequest.MappedName}
                onInput={e => this.newFolderRequest = {
                  ...this.newFolderRequest,
                  MappedName: (e.target as HTMLInputElement).value,
                }}
              />
            ]}
          </div>
          <div class="controls">
            <dnn-button
              appearance="primary"
              reversed
              onClick={() => void this.handleCancel()}
            >
              {state.localization.Cancel}
            </dnn-button>
            <dnn-button
              appearance="primary"
              type="submit"
            >
              {state.localization.Save}
            </dnn-button>
          </div>
        </form>
      </Host>
    );
  }
}
