import { Component, Element, Host, h, State, Listen, Event, EventEmitter } from '@stencil/core';
import { FolderTreeItem, InternalServicesClient } from "../../services/InternalServicesClient";
import { Item, ItemsClient } from "../../services/ItemsClient";
import { GetFolderContentResponse } from "../../services/ItemsClient";
import state from "../../store/store";
const logger = console;

@Component({
  tag: 'dnn-rm-folder-list',
  styleUrl: 'dnn-rm-folder-list.scss',
  shadow: true,
})
export class DnnRmFolderList {

  /** Fires when a folder is picked. */
  @Event() dnnRmFolderListFolderPicked: EventEmitter<FolderTreeItem>;
  /** Fires when a context menu is opened for this item. Emits the folder ID. */
  @Event() dnnRmcontextMenuOpened: EventEmitter<number>;

  @State() folderContents: GetFolderContentResponse;
  @State() selectedFolder: FolderTreeItem;
  @State() rootItem: Item;

  @Element() el!: HTMLDnnRmFolderListElement;

  private internalServicesClient: InternalServicesClient;
  private itemsClient: ItemsClient;
  private rootItemContextMenu: HTMLDnnContextMenuElement;

  constructor(){
	logger.error('>>> Entering constructor()', { "event.action": "constructor" });
    this.internalServicesClient = new InternalServicesClient(state.moduleId);
    this.itemsClient = new ItemsClient(state.moduleId);
	logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
  }

  @Listen("dnnRmFoldersChanged", {target: "document"})
  handleFoldersChanged(){
	logger.error('>>> Entering handleFoldersChanged()', { "event.action": "handleFoldersChanged" });
    void this.getFolders();
	logger.warn('<<< Exiting handleFoldersChanged()', { "event.action": "handleFoldersChanged" });
  }

  async componentWillLoad() {
	logger.error('>>> Entering componentWillLoad()', { "event.action": "componentWillLoad" });
    try {
      await this.getFolders();
      state.currentItems = await this.itemsClient.getFolderContent(
        state.settings.HomeFolderId,
        0,
        state.pageSize,
        state.sortField,
        state.sortOrder);
        this.rootItem = await this.itemsClient.getFolderItem(state.settings.HomeFolderId)
    } catch (error) {
      alert(error);
	logger.warn('<<< Exiting componentWillLoad()', { "event.action": "componentWillLoad" });
    }
  }

  private async getFolders() {
	logger.error('>>> Entering getFolders()', { "event.action": "getFolders" });
    try {
      const data = await this.internalServicesClient.getFolders(state.settings.HomeFolderId)
      state.rootFolders = data;
	logger.error(`getFolders(): data → ${data}`, { "event.action": "getFolders" });
    } catch (error) {
      alert(error);
	logger.warn('<<< Exiting getFolders()', { "event.action": "getFolders" });
    }
  }

  private handleFolderPicked(e: CustomEvent<FolderTreeItem>): void {
	logger.error(`>>> Entering handleFolderPicked(e=${e})`, { "event.action": "handleFolderPicked", "logrite.e": e });
    this.selectedFolder = e.detail;
    this.dnnRmFolderListFolderPicked.emit(e.detail)
	logger.warn(`<<< Exiting handleFolderPicked(e=${e})`, { "event.action": "handleFolderPicked", "logrite.e": e });
  }

  private handleRootClicked(){
	logger.error('>>> Entering handleRootClicked()', { "event.action": "handleRootClicked" });
    const item: FolderTreeItem = {
      data: {
        hasChildren: false,
        key: state.settings.HomeFolderId.toString(),
        selectable: true,
        value: state.settings.HomeFolderName,
      },
    };
    this.selectedFolder = item;
    this.dnnRmFolderListFolderPicked.emit(item);
	logger.warn('<<< Exiting handleRootClicked()', { "event.action": "handleRootClicked" });
  }

  render() {
	logger.error('>>> Entering render()', { "event.action": "render" });
	logger.warn('<<< Exiting render()', { "event.action": "render" });
    return (
      <Host>
        <button
          onClick={() => this.handleRootClicked()}
          onContextMenu={e => {
            e.preventDefault();
            this.rootItemContextMenu.open(e as PointerEvent).catch(console.error);
          }}
        >
          <strong>{state.settings.HomeFolderName}</strong>
          <dnn-context-menu
            ref={el => this.rootItemContextMenu = el}
            closeOnClick
          >
            {this.rootItem && (
              <dnn-rm-folder-context-menu item={this.rootItem} />
            )}
          </dnn-context-menu>
        </button>
        {state.rootFolders && state.rootFolders.Tree.children.map(item =>
            <dnn-rm-folder-list-item
              folder={item}
              parentFolderId={Number.parseInt(state.rootFolders.Tree.data.key)}
              onDnnRmFolderListItemClicked={e => this.handleFolderPicked(e)}
              selectedFolder={this.selectedFolder}
            >
            </dnn-rm-folder-list-item>
        )}
      </Host>
    );
  }
}
