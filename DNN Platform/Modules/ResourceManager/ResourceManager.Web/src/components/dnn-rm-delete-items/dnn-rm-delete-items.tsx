import { Component, Element, Event, EventEmitter, Host, h, Prop, State } from '@stencil/core';
import { Item, ItemsClient } from '../../services/ItemsClient';
import state from '../../store/store';
const logger = console;

@Component({
  tag: 'dnn-rm-delete-items',
  styleUrl: 'dnn-rm-delete-items.scss',
  shadow: true,
})
export class DnnRmDeleteItems {

  /** The list of items to delete. */
  @Prop() items!: Item[];

  /**
  * Fires when there is a possibility that some folders have changed.
  * Can be used to force parts of the UI to refresh.
  */
  @Event() dnnRmFoldersChanged: EventEmitter<void>;
  
  @Element() el: HTMLDnnRmDeleteItemsElement;
  
  @State() deleting: boolean;
  @State() deletedCount: number = 0;
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

  private async handleDelete() {
	logger.debug('>>> Entering handleDelete()');
    this.deleting = true;
    try {
      for (const item of this.items){
        if (item.isFolder){
          await this.itemsClient.deleteFolder({
            FolderId: item.itemId,
            UnlinkAllowedStatus: false,
          });
          await this.handleItemDeleted(item);
        }
        else {
          await this.itemsClient.deleteFile({
            FileId: item.itemId,
          });
          await this.handleItemDeleted(item);
        }
      }
    } catch (error) {
      alert(error);
	logger.debug('<<< Exiting handleDelete()');
    }
  }
  
  private async handleItemDeleted(item: Item) {
	logger.debug(`>>> Entering handleItemDeleted(item=${item})`);
    this.deletedCount++;
    this.currentItemName = item.itemName;
    if (this.deletedCount == this.items.length) {
	logger.debug(`handleItemDeleted(item=${item}): this.currentItemName → ${this.currentItemName}`);
      this.deleting = false;
      this.dnnRmFoldersChanged.emit();
      await this.closeModal();
	logger.debug(`<<< Exiting handleItemDeleted(item=${item})`);
    }
  }

  render() {
	logger.debug('>>> Entering render()');
	logger.debug('<<< Exiting render()');
    return (
      <Host>
        <h2>{state.localization?.DeleteItems}</h2>
        <p>{state.localization?.ConfirmDeleteMessage}</p>
        <ul>
          {this.items.map(item => (
            <li>{item.itemName}</li>
          ))}
        </ul>
        {this.deleting &&
          <div>
            <p>{state.localization?.DeletingItems} {this.deletedCount+1}/{state.selectedItems.length} ({this.currentItemName})</p>
            <dnn-rm-progress-bar
              max={state.selectedItems.length}
              value={this.deletedCount}
            />
          </div>
        }
        <div class="controls">
          <dnn-button
            appearance="primary"
            reversed
            disabled={this.deleting}
            onClick={() => void this.closeModal()}
          >
            {state.localization.Cancel}
          </dnn-button>
          <dnn-button
            appearance="primary"
            onClick={() => void this.handleDelete()}
            disabled={this.deleting}
          >
            {state.localization?.Delete}
          </dnn-button>
        </div>
      </Host>
    );
  }
}
