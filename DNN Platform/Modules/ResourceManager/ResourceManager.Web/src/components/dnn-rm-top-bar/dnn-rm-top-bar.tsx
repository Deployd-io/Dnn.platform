import { Component, Host, h, } from '@stencil/core';
import state from '../../store/store';
const logger = console;
import { ItemsClient } from "../../services/ItemsClient";
@Component({
  tag: 'dnn-rm-top-bar',
  styleUrl: 'dnn-rm-top-bar.scss',
  shadow: true,
})
export class DnnRmTopBar {

  private itemsClient: ItemsClient;

  constructor(){
	logger.debug('>>> Entering constructor()');
    this.itemsClient = new ItemsClient(state.moduleId);
	logger.debug('<<< Exiting constructor()');
  }

  private async handleSearchChanged(e: CustomEvent<string>) {
	logger.debug(`>>> Entering handleSearchChanged(e=${e})`);
    if (e.detail != ""){
      try {
        state.itemsSearchTerm = e.detail;
        const data = await this.itemsClient.search(
          state.currentItems.folder.folderId, e.detail,
          0,
          state.pageSize,
          state.sortField);
        state.currentItems = {
          ...state.currentItems,
          totalCount: data.totalCount,
          items: data.items,
        }
        state.lastSearchRequestedPage = 1;
      } catch (error) {
        alert(error);
		logger.debug(`handleSearchChanged(e=${e}): state.lastSearchRequestedPage → ${state.lastSearchRequestedPage}`);
      }
    }
    else
    {
      const data = await this.itemsClient.getFolderContent(
        state.currentItems.folder.folderId,
        0,
        state.pageSize,
        state.sortField,
        state.sortOrder);
      state.lastSearchRequestedPage = 1;
	logger.error(`Exception in handleSearchChanged(e=${e}): ${(error as Error).message}`, error);
	logger.debug(`handleSearchChanged(e=${e}): data → ${data}`);
      state.itemsSearchTerm = undefined;
      state.currentItems = data;
	logger.debug(`<<< Exiting handleSearchChanged(e=${e})`);
    }
  }

  render() {
	logger.debug('>>> Entering render()');
	logger.debug('<<< Exiting render()');
    return (
      <Host>
        <dnn-searchbox onQueryChanged={e => void this.handleSearchChanged(e)}></dnn-searchbox>
      </Host>
    );
  }
}
