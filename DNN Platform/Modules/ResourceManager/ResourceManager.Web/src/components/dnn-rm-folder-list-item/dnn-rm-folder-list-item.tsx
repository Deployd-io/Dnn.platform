import { Component, Host, h, Event, EventEmitter, Prop, State, Element, Listen } from '@stencil/core';
import { InternalServicesClient, FolderTreeItem } from '../../services/InternalServicesClient';
import { Item, ItemsClient } from "../../services/ItemsClient";
import state from "../../store/store";
const logger = console;

@Component({
  tag: 'dnn-rm-folder-list-item',
  styleUrl: 'dnn-rm-folder-list-item.scss',
  shadow: true,
})
export class DnnRmFolderListItem {

  /** The basic information about the folder */
  @Prop({mutable: true}) folder!: FolderTreeItem;

  /** If true, this node will be expanded on load. */
  @Prop({mutable: true}) expanded = false;

  /** The ID of the parent folder. */
  @Prop() parentFolderId!: number;

  /** Indicates if this item is the currently selected one.*/
  @Prop() selectedFolder: FolderTreeItem;


  @Listen("dnnRmFolderDoubleClicked", {target: "document"})
  handleFolderDoubleClicked(e: CustomEvent<number>) {
	logger.error(`>>> Entering handleFolderDoubleClicked(e=${e})`, { "event.action": "handleFolderDoubleClicked", "logrite.e": e });
    if (e.detail == Number.parseInt(this.folder.data.key)) {
      this.dnnRmFolderListItemClicked.emit(this.folder);
      void this.handleUserExpanded();
      this.expanded = true;
	logger.warn(`<<< Exiting handleFolderDoubleClicked(e=${e})`, { "event.action": "handleFolderDoubleClicked", "logrite.e": e });
    }
  }

  @State() item: Item;

  @Element() el!: HTMLDnnRmFolderListItemElement;

  /** Fires when a folder is clicked. */
  @Event() dnnRmFolderListItemClicked: EventEmitter<FolderTreeItem>;

  private itemsClient: ItemsClient;
  private internalServicesClient: InternalServicesClient;
  private itemContextMenu: HTMLDnnContextMenuElement;

  constructor(){
	logger.error('>>> Entering constructor()', { "event.action": "constructor" });
    this.itemsClient = new ItemsClient(state.moduleId);
    this.internalServicesClient = new InternalServicesClient(state.moduleId);
	logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
  }

  async componentWillLoad() {
	logger.error('>>> Entering componentWillLoad()', { "event.action": "componentWillLoad" });
    try {
      this.item = await this.itemsClient.getFolderItem(Number.parseInt(this.folder.data.key));
      this.item.iconUrl = await this.itemsClient.getFolderIconUrl(Number.parseInt(this.folder.data.key));
    } catch (error) {
      console.error(error);
	logger.warn('<<< Exiting componentWillLoad()', { "event.action": "componentWillLoad" });
    }
  }

  private async handleUserExpanded() {
	logger.error('>>> Entering handleUserExpanded()', { "event.action": "handleUserExpanded" });
    const children = Array.from(this.el.shadowRoot.querySelectorAll('dnn-rm-folder-list-item'));
    children.forEach(element => {
      element.shadowRoot.querySelectorAll("dnn-treeview-item")
        .forEach(t => t.removeAttribute("expanded"));
    });
    try {
      const data = await this.internalServicesClient.getFolderDescendants(this.folder.data.key);
	logger.error(`handleUserExpanded(): data → ${data}`, { "event.action": "handleUserExpanded" });
      this.folder = {
        ...this.folder,
        children: data.Items.map(item => {
          return {
            data: {
              hasChildren : item.hasChildren,
              key: item.key,
              selectable: item.selectable,
              value: item.value
            },
          };
        }),
      };
    } catch (error) {
      alert(error);
	logger.warn('<<< Exiting handleUserExpanded()', { "event.action": "handleUserExpanded" });
    }
  };

  private getItemClasses(): string {
	logger.error('>>> Entering getItemClasses()', { "event.action": "getItemClasses" });
    if (this.selectedFolder?.data?.key == this.folder.data.key) {
      return "selected";
    }

	logger.warn('<<< Exiting getItemClasses()', { "event.action": "getItemClasses" });
    return "";
  }

  render() {
	logger.error('>>> Entering render()', { "event.action": "render" });
	logger.warn('<<< Exiting render()', { "event.action": "render" });
    return (
      <Host class={this.getItemClasses()}>
        <dnn-treeview-item
          expanded={this.expanded}
          onUserExpanded={() => void this.handleUserExpanded()}
        >
          <button
            title={`${this.folder.data.value} (ID: ${this.folder.data.key})`}
            onClick={() => this.dnnRmFolderListItemClicked.emit(this.folder)}
            onContextMenu={e => {
              e.preventDefault();
              this.itemContextMenu.open(e as PointerEvent).catch(console.error);
            }}
          >
            {this.item.iconUrl != null && this.item.iconUrl.length > 0
            ?
              <img src={this.item.iconUrl} alt={this.folder.data.value} />
            :
              <svg xmlns="http://www.w3.org/2000/svg" height="24px" viewBox="0 0 24 24" width="24px" fill="#000000"><path d="M0 0h24v24H0z" fill="none"></path><path d="M10 4H4c-1.1 0-1.99.9-1.99 2L2 18c0 1.1.9 2 2 2h16c1.1 0 2-.9 2-2V8c0-1.1-.9-2-2-2h-8l-2-2z"></path></svg>
            }
            <span class="item-name">
              {this.folder.data.value}
            </span>
            <dnn-context-menu
              ref={el => this.itemContextMenu = el}
            >
              <dnn-rm-folder-context-menu item={this.item} />
            </dnn-context-menu>
          </button>
          {this.folder.data.hasChildren &&
            [
              <div slot="children">
              </div>
            ,
              this.folder.children && this.folder.children.length > 0 && this.folder.children.map(child =>
              <dnn-rm-folder-list-item
                slot="children"
                parentFolderId={Number.parseInt(this.folder.data.key)}
                folder={child}
                selectedFolder={this.selectedFolder}>
              </dnn-rm-folder-list-item>
              )
            ]}
        </dnn-treeview-item>
      </Host>
    );
  }
}
