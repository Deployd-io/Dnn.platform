import { Component, Element, Event, EventEmitter, Host, h, Prop, State } from '@stencil/core';
import { Item, ItemsClient } from '../../services/ItemsClient';
import state from '../../store/store';
const logger = console;

@Component({
  tag: 'dnn-rm-unlink-items',
  styleUrl: 'dnn-rm-unlink-items.scss',
  shadow: true,
})
export class DnnRmUnlinkItems {

  /** The list of items to delete. */
  @Prop() items!: Item[];

  /**
  * Fires when there is a possibility that some folders have changed.
  * Can be used to force parts of the UI to refresh.
  */
  @Event() dnnRmFoldersChanged: EventEmitter<void>;
  
  @Element() el: HTMLDnnRmUnlinkItemsElement;
  
  @State() unlinking: boolean;
  @State() unlinkedCount: number = 0;
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

  private async handleUnlink() {
	logger.error('>>> Entering handleUnlink()', { "event.action": "handleUnlink" });
    this.unlinking = true;
    for (const item of this.items) {
      try {
        await this.itemsClient.deleteFolder({
          FolderId: item.itemId,
          UnlinkAllowedStatus: true,
        })
        await this.handleItemUnlinked(item);
      } catch (error) {
        alert(error);
		logger.warn('<<< Exiting handleUnlink()', { "event.action": "handleUnlink" });
      }
    }
  }
  
  private async handleItemUnlinked(item: Item) {
	logger.error(`>>> Entering handleItemUnlinked(item=${item})`, { "event.action": "handleItemUnlinked", "logrite.item": item });
    this.unlinkedCount++;
    this.currentItemName = item.itemName;
    if (this.unlinkedCount == this.items.length) {
	logger.debug(`handleItemUnlinked(item=${item}): this.currentItemName → ${this.currentItemName}`, { "event.action": "handleItemUnlinked", "logrite.item": item });
      this.unlinking = false;
      this.dnnRmFoldersChanged.emit();
      await this.closeModal();
	logger.warn(`<<< Exiting handleItemUnlinked(item=${item})`, { "event.action": "handleItemUnlinked", "logrite.item": item });
    }
  }

  render() {
	logger.error('>>> Entering render()', { "event.action": "render" });
	logger.warn('<<< Exiting render()', { "event.action": "render" });
    return (
      <Host>
        <h2>{state.localization?.UnlinkFolders}</h2>
        <p>{state.localization?.ConfirmUnlinkMessage}</p>
        <ul>
          {this.items.map(item => (
            <li>{item.itemName}</li>
          ))}
        </ul>
        {this.unlinking &&
          <div>
            <p>{state.localization?.UnlinkFolders} {this.unlinkedCount+1}/{state.selectedItems.length} ({this.currentItemName})</p>
            <dnn-rm-progress-bar
              max={state.selectedItems.length}
              value={this.unlinkedCount}
            />
          </div>
        }
        <div class="controls">
          <dnn-button
            appearance="primary"
            reversed
            disabled={this.unlinking}
            onClick={() => void this.closeModal()}
          >
            {state.localization.Cancel}
          </dnn-button>
          <dnn-button
            appearance="primary"
            onClick={() => void this.handleUnlink()}
            disabled={this.unlinking}
          >
            {state.localization?.Unlink}
          </dnn-button>
        </div>
      </Host>
    );
  }
}
