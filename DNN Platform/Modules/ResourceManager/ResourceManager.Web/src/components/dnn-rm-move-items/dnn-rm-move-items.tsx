import { Component, Element, Event, EventEmitter, Host, h, Prop, State } from '@stencil/core';
import { FolderTreeItem } from '../../services/InternalServicesClient';
import { Item, ItemsClient } from '../../services/ItemsClient';
import state from '../../store/store';
const logger = console;
// import { ItemsClient } from '../../services/ItemsClient';

@Component({
  tag: 'dnn-rm-move-items',
  styleUrl: 'dnn-rm-move-items.scss',
  shadow: true,
})
export class DnnRmMoveItems {

  /** The list of items to delete. */
  @Prop() items!: Item[];

  /**
  * Fires when there is a possibility that some folders have changed.
  * Can be used to force parts of the UI to refresh.
  */
  @Event() dnnRmFoldersChanged: EventEmitter<void>;

  @State() selectedFolder: FolderTreeItem;
  
  @Element() el: HTMLDnnRmMoveItemsElement;
  
  @State() moving: boolean;
  @State() movedCount: number = 0;
  @State() currentItemName: string;
  
  private itemsClient: ItemsClient;

  constructor() {
	logger.debug('>>> Entering constructor()');
    this.itemsClient = new ItemsClient(state.moduleId);
	logger.debug('<<< Exiting constructor()');
  }

  private async closeModal() {
	logger.debug('>>> Entering closeModal()');
    const modal = this.el.parentElement as HTMLDnnModalElement;
    await modal.hide();
    setTimeout(() => {
      document.body.removeChild(modal);
    }, 300);
	logger.debug('<<< Exiting closeModal()');
  }

  private async handleMove() {
	logger.debug('>>> Entering handleMove()');
    this.moving = true;
    for (const item of this.items){
      if (item.isFolder){
        try {
          await this.itemsClient.moveFolder({
            SourceFolderId: item.itemId,
            DestinationFolderId: Number.parseInt(this.selectedFolder.data.key)});
          await this.handleItemMoved(item);
        } catch (error) {
          alert(error);
        }
      }
      else {
        try {
          await this.itemsClient.moveFile({
            SourceFileId: item.itemId,
            DestinationFolderId: Number.parseInt(this.selectedFolder.data.key)});
            await this.handleItemMoved(item);
        } catch (error) {
          alert(error);
		logger.debug('<<< Exiting handleMove()');
        }
      }
    }
  }
  
  private async handleItemMoved(item: Item) {
	logger.debug(`>>> Entering handleItemMoved(item=${item})`);
    this.movedCount++;
    this.currentItemName = item.itemName;
    if (this.movedCount == this.items.length) {
	logger.debug(`handleItemMoved(item=${item}): this.currentItemName → ${this.currentItemName}`);
      this.moving = false;
      this.dnnRmFoldersChanged.emit();
      await this.closeModal();
	logger.debug(`<<< Exiting handleItemMoved(item=${item})`);
    }
  }

  private getMoveButtonText() {
	logger.debug('>>> Entering getMoveButtonText()');
	logger.debug('<<< Exiting getMoveButtonText()');
      return this.selectedFolder == undefined ? state.localization?.NoFolderSelected : state.localization?.Move;
  }

  render() {
	logger.debug('>>> Entering render()');
	logger.debug('<<< Exiting render()');
    return (
      <Host>
        <h2>{state.localization?.MoveItems}</h2>
        <p>{state.localization?.SelectDestinationFolder}</p>
        <dnn-rm-folder-list
          onDnnRmFolderListFolderPicked={e => this.selectedFolder=e.detail}
        />
        {this.moving &&
          <div>
            <p>{state.localization?.MovingItems} {this.movedCount+1}/{state.selectedItems.length} ({this.currentItemName})</p>
            <dnn-rm-progress-bar
              max={state.selectedItems.length}
              value={this.movedCount}
            />
          </div>
        }
        <div class="controls">
          <dnn-button
            appearance="primary"
            reversed
            disabled={this.moving}
            onClick={() => void this.closeModal()}
          >
            {state.localization.Cancel}
          </dnn-button>
          <dnn-button
            appearance="primary"
            onConfirmed={() => void this.handleMove()}
            confirm
            confirmMessage={state.localization?.ConfirmMoveMessage}
            confirmYesText={state.localization?.Yes}
            confirmNoText={state.localization?.No}
            disabled={this.moving}
          >
            {this.getMoveButtonText()}
          </dnn-button>
        </div>
      </Host>
    );
  }
}
