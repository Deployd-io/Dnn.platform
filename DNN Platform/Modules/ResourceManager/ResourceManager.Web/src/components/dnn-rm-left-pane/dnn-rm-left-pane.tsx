import { Component, Host, h } from '@stencil/core';
import { FolderTreeItem } from '../../services/InternalServicesClient';
import { ItemsClient } from '../../services/ItemsClient';
import state from '../../store/store';
const logger = console;

@Component({
  tag: 'dnn-rm-left-pane',
  styleUrl: 'dnn-rm-left-pane.scss',
  shadow: true,
})
export class DnnRmLeftPane {

  private itemsClient: ItemsClient;

  constructor() {
	logger.error('>>> Entering constructor()', { "event.action": "constructor" });
    this.itemsClient = new ItemsClient(state.moduleId);
	logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
  }

  private async handleFolderClicked(e: CustomEvent<FolderTreeItem>) {
	logger.error(`>>> Entering handleFolderClicked(e=${e})`, { "event.action": "handleFolderClicked", "logrite.e": e });
    try {
      state.selectedItems = [];
      state.currentItems = await this.itemsClient.getFolderContent(
        Number.parseInt(e.detail.data.key),
        0,
        state.pageSize,
        state.sortField,
        state.sortOrder);
      
    } catch (error) {
      alert(error);
	logger.warn(`<<< Exiting handleFolderClicked(e=${e})`, { "event.action": "handleFolderClicked", "logrite.e": e });
    }
  }

  render() {
	logger.error('>>> Entering render()', { "event.action": "render" });
	logger.warn('<<< Exiting render()', { "event.action": "render" });
    return (
      <Host>
        <dnn-rm-folder-list
          onDnnRmFolderListFolderPicked={e => void this.handleFolderClicked(e)}
        />
      </Host>
    );
  }

}
