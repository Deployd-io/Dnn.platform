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
	logger.error('>>> Entering constructor()', { "event.action": "constructor" });
    this.itemsClient = new ItemsClient(state.moduleId);
	logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
  }

  private async handleSearchChanged(e: CustomEvent<string>) {
	logger.error(`>>> Entering handleSearchChanged(e=${e})`, { "event.action": "handleSearchChanged", "logrite.e": e });
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
	logger.error(`Exception in handleSearchChanged(e=${e}): ${(error as Error).message}`, error, { "event.action": "handleSearchChanged", "event.category": "process", "logrite.e": e });
	logger.error(`handleSearchChanged(e=${e}): data → ${data}`, { "event.action": "handleSearchChanged", "logrite.e": e });
      state.itemsSearchTerm = undefined;
      state.currentItems = data;
	logger.warn(`<<< Exiting handleSearchChanged(e=${e})`, { "event.action": "handleSearchChanged", "logrite.e": e });
    }
  }

  render() {
	logger.error('>>> Entering render()', { "event.action": "render" });
	logger.warn('<<< Exiting render()', { "event.action": "render" });
    return (
      <Host>
        <dnn-searchbox onQueryChanged={e => void this.handleSearchChanged(e)}></dnn-searchbox>
      </Host>
    );
  }
}
