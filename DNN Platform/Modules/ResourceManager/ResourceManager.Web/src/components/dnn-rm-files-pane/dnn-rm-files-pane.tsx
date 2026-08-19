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
	logger.debug('>>> Entering constructor()');
    this.itemsClient = new ItemsClient(state.moduleId);
	logger.debug('<<< Exiting constructor()');
  }

  @Listen("scroll")
  handleScroll(){
	logger.debug('>>> Entering handleScroll()');
    void this.checkIfMoreItemsNeeded();
	logger.debug('<<< Exiting handleScroll()');
  }

  @Listen("dnnRmFoldersChanged", {target: "document"})
  handleFoldersChanged(){
	logger.debug('>>> Entering handleFoldersChanged()');
    state.currentItems = {
      ...state.currentItems,
      items: [],
    };
    state.selectedItems = [];
	logger.debug(`handleFoldersChanged(): state.currentItems → ${state.currentItems}`);
    void this.checkIfMoreItemsNeeded();
	logger.debug('<<< Exiting handleFoldersChanged()');
  }

  @Listen("dnnRmFileDoubleClicked", {target: "document"})
  handleFileDoubleClicked(e: CustomEvent<string>) {
	logger.debug(`>>> Entering handleFileDoubleClicked(e=${e})`);
    window.open(e.detail, "_blank");
	logger.debug(`<<< Exiting handleFileDoubleClicked(e=${e})`);
  }

  componentDidUpdate() {
	logger.debug('>>> Entering componentDidUpdate()');
    const loadedFilesHeight = this.loadedFilesArea.getBoundingClientRect().height;
    const heightPerItem = loadedFilesHeight / state.currentItems.items.length;
	logger.debug(`componentDidUpdate(): loadedFilesHeight → ${loadedFilesHeight}`);
    const totalNeededHeight = heightPerItem * state.currentItems.totalCount;
    let spacerHeight = totalNeededHeight - loadedFilesHeight;
	logger.debug(`componentDidUpdate(): spacerHeight → ${spacerHeight}`);
    if (spacerHeight < 0){
      spacerHeight = 0;
    }
    this.spacer.style.height = `${spacerHeight}px`;

    void this.checkIfMoreItemsNeeded();
	logger.debug('<<< Exiting componentDidUpdate()');
  }

  private async checkIfMoreItemsNeeded() {
	logger.debug('>>> Entering checkIfMoreItemsNeeded()');
    const endOfFilesHeight = this.spacer.getBoundingClientRect().top;
    const thisBottom = this.el.getBoundingClientRect().bottom;
	logger.debug(`checkIfMoreItemsNeeded(): endOfFilesHeight → ${endOfFilesHeight}`);
	logger.debug(`checkIfMoreItemsNeeded(): thisBottom → ${thisBottom}`);
    const offset = endOfFilesHeight - thisBottom;
    if (offset < this.preloadOffset){
      await this.loadMore();
	logger.debug('<<< Exiting checkIfMoreItemsNeeded()');
    }
  }

  @Debounce(50)
  private async loadMore() {
	logger.debug('>>> Entering loadMore()');
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
		logger.debug(`loadMore(): data → ${data}`);
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
	logger.debug(`loadMore(): state.currentItems → ${state.currentItems}`);
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
	logger.debug('<<< Exiting loadMore()');
  }

  render() {
	logger.debug('>>> Entering render()');
	logger.debug('<<< Exiting render()');
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
