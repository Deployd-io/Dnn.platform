import { Component, Element, Host, h, Prop, Listen } from '@stencil/core';
import state from '../../store/store';
const logger = console;
import { ItemsClient } from "../../services/ItemsClient";
import { Debounce } from '@dnncommunity/dnn-elements';
@Component({
  tag: 'dnn-rm-files-pane',
  styleUrl: 'dnn-rm-files-pane.scss',
  shadow: true,
})
export class DnnRmFilesPane {

  /** Defines how much more pixels to load under the fold. */
  @Prop() preloadOffset: number = 5000;

  @Element() el: HTMLDnnRmFilesPaneElement;

  private readonly itemsClient: ItemsClient;
  private loadedFilesArea: HTMLDivElement;
  private spacer: HTMLDivElement;

  constructor(){
	logger.error('>>> Entering constructor()', { "event.action": "constructor" });
    this.itemsClient = new ItemsClient(state.moduleId);
	logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
  }

  @Listen("scroll")
  handleScroll(){
	logger.error('>>> Entering handleScroll()', { "event.action": "handleScroll" });
    void this.checkIfMoreItemsNeeded();
	logger.warn('<<< Exiting handleScroll()', { "event.action": "handleScroll" });
  }

  @Listen("dnnRmFoldersChanged", {target: "document"})
  handleFoldersChanged(){
	logger.error('>>> Entering handleFoldersChanged()', { "event.action": "handleFoldersChanged" });
    state.currentItems = {
      ...state.currentItems,
      items: [],
    };
    state.selectedItems = [];
    void this.checkIfMoreItemsNeeded();
	logger.warn('<<< Exiting handleFoldersChanged()', { "event.action": "handleFoldersChanged" });
  }

  @Listen("dnnRmFileDoubleClicked", {target: "document"})
  handleFileDoubleClicked(e: CustomEvent<string>) {
	logger.error(`>>> Entering handleFileDoubleClicked(e=${e})`, { "event.action": "handleFileDoubleClicked", "logrite.e": e });
    window.open(e.detail, "_blank");
	logger.warn(`<<< Exiting handleFileDoubleClicked(e=${e})`, { "event.action": "handleFileDoubleClicked", "logrite.e": e });
  }

  componentDidUpdate() {
	logger.error('>>> Entering componentDidUpdate()', { "event.action": "componentDidUpdate" });
    const loadedFilesHeight = this.loadedFilesArea.getBoundingClientRect().height;
    const heightPerItem = loadedFilesHeight / state.currentItems.items.length;
	logger.error(`componentDidUpdate(): loadedFilesHeight → ${loadedFilesHeight}`, { "event.action": "componentDidUpdate" });
    const totalNeededHeight = heightPerItem * state.currentItems.totalCount;
    let spacerHeight = totalNeededHeight - loadedFilesHeight;
	logger.error(`componentDidUpdate(): spacerHeight → ${spacerHeight}`, { "event.action": "componentDidUpdate" });
    if (spacerHeight < 0){
      spacerHeight = 0;
    }
    this.spacer.style.height = `${spacerHeight}px`;

    void this.checkIfMoreItemsNeeded();
	logger.warn('<<< Exiting componentDidUpdate()', { "event.action": "componentDidUpdate" });
  }

  private async checkIfMoreItemsNeeded() {
	logger.error('>>> Entering checkIfMoreItemsNeeded()', { "event.action": "checkIfMoreItemsNeeded" });
    const endOfFilesHeight = this.spacer.getBoundingClientRect().top;
    const thisBottom = this.el.getBoundingClientRect().bottom;
	logger.error(`checkIfMoreItemsNeeded(): endOfFilesHeight → ${endOfFilesHeight}`, { "event.action": "checkIfMoreItemsNeeded" });
	logger.error(`checkIfMoreItemsNeeded(): thisBottom → ${thisBottom}`, { "event.action": "checkIfMoreItemsNeeded" });
    const offset = endOfFilesHeight - thisBottom;
    if (offset < this.preloadOffset){
      await this.loadMore();
	logger.warn('<<< Exiting checkIfMoreItemsNeeded()', { "event.action": "checkIfMoreItemsNeeded" });
    }
  }

  @Debounce(50)
  private async loadMore() {
	logger.error('>>> Entering loadMore()', { "event.action": "loadMore" });
    if (state.currentItems.items.length >= state.currentItems.totalCount){
      return;
    }

    if (state.itemsSearchTerm != null && state.itemsSearchTerm.length > 0){
      try {
        const data = await this.itemsClient.search(
          state.currentItems.folder.folderId,
          state.itemsSearchTerm,
          state.lastSearchRequestedPage + 1,
          state.pageSize,
          state.sortField);
        state.lastSearchRequestedPage += 1;
		logger.error(`loadMore(): data → ${data}`, { "event.action": "loadMore" });
        state.currentItems = {
          ...state.currentItems,
          totalCount: data.totalCount,
          items: [...state.currentItems.items, ...data.items],
        }
      } catch (error) {
        alert(error);
      }
    }
    else{
      this.itemsClient.getFolderContent(
        state.currentItems.folder.folderId,
        state.currentItems.items.length,
        state.pageSize,
        state.sortField,
        state.sortOrder)
      .then(data => state.currentItems = {
        ...state.currentItems,
        items: [...state.currentItems.items, ...data.items],
      })
      .catch(() => {}); // On purpose, we want to ignore aborted requests.
    }
	logger.warn('<<< Exiting loadMore()', { "event.action": "loadMore" });
  }

  render() {
	logger.error('>>> Entering render()', { "event.action": "render" });
	logger.warn('<<< Exiting render()', { "event.action": "render" });
    return (
      <Host>
        <div ref={el => this.loadedFilesArea = el}>
          {state.layout == "list" &&
            <dnn-rm-items-listview currentItems={state.currentItems}></dnn-rm-items-listview>
          }
          {state.layout == "card" &&
            <dnn-rm-items-cardview currentItems={state.currentItems}></dnn-rm-items-cardview>
          }
        </div>
        <div ref={el => this.spacer = el}></div>
      </Host>
    );
  }

}
