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
	logger.error('>>> Entering constructor()', { "event.action": "constructor" });
    this.itemsClient = new ItemsClient(state.moduleId);
	logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
  }

  private async closeModal() {
	logger.error('>>> Entering closeModal()', { "event.action": "closeModal" });
    const modal = this.el.parentElement as HTMLDnnModalElement;
    await modal.hide();
    setTimeout(() => {
      document.body.removeChild(modal);
    }, 300);
	logger.warn('<<< Exiting closeModal()', { "event.action": "closeModal" });
  }

  private async handleMove() {
	logger.error('>>> Entering handleMove()', { "event.action": "handleMove" });
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
		logger.warn('<<< Exiting handleMove()', { "event.action": "handleMove" });
        }
      }
    }
  }
  
  private async handleItemMoved(item: Item) {
	logger.error(`>>> Entering handleItemMoved(item=${item})`, { "event.action": "handleItemMoved", "logrite.item": item });
    this.movedCount++;
    this.currentItemName = item.itemName;
    if (this.movedCount == this.items.length) {
	logger.debug(`handleItemMoved(item=${item}): this.currentItemName → ${this.currentItemName}`, { "event.action": "handleItemMoved", "logrite.item": item });
      this.moving = false;
      this.dnnRmFoldersChanged.emit();
      await this.closeModal();
	logger.warn(`<<< Exiting handleItemMoved(item=${item})`, { "event.action": "handleItemMoved", "logrite.item": item });
    }
  }

  private getMoveButtonText() {
	logger.error('>>> Entering getMoveButtonText()', { "event.action": "getMoveButtonText" });
	logger.warn('<<< Exiting getMoveButtonText()', { "event.action": "getMoveButtonText" });
      return this.selectedFolder == undefined ? state.localization?.NoFolderSelected : state.localization?.Move;
  }

  render() {
	logger.error('>>> Entering render()', { "event.action": "render" });
	logger.warn('<<< Exiting render()', { "event.action": "render" });
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
