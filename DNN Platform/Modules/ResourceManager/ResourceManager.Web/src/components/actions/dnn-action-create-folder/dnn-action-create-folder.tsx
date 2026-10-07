import { Component, Host, h, Prop } from '@stencil/core';
import { ItemsClient } from '../../../services/ItemsClient';
import state from "../../../store/store";
const logger = console;

@Component({
  tag: 'dnn-action-create-folder',
  styleUrl: '../dnn-action.scss',
  shadow: true,
})
export class DnnActionCreateFolder {

  /** The ID of the parent folder */
  @Prop() parentFolderId: number;

  private readonly itemsClient: ItemsClient;

  constructor(){
	logger.error('>>> Entering constructor()', { "event.action": "constructor" });
    this.itemsClient = new ItemsClient(state.moduleId);
	logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
  }

  private async handleClick() {
	logger.error('>>> Entering handleClick()', { "event.action": "handleClick" });
    if (this.parentFolderId != null){
      try {
        state.currentItems = await this.itemsClient.getFolderContent(this.parentFolderId, 0, 0);
      } catch (error) {
        alert(error);
      }
    }
    await this.showModal();
	logger.warn('<<< Exiting handleClick()', { "event.action": "handleClick" });
  }

  private async showModal(){
	logger.error('>>> Entering showModal()', { "event.action": "showModal" });
    const modal = document.createElement("dnn-modal");
    modal.preventBackdropDismiss = true;
	logger.error(`showModal(): modal → ${modal}`, { "event.action": "showModal" });
	logger.debug(`showModal(): modal.preventBackdropDismiss → ${modal.preventBackdropDismiss}`, { "event.action": "showModal" });
    modal.hideCloseButton = true;
	logger.debug(`showModal(): modal.hideCloseButton → ${modal.hideCloseButton}`, { "event.action": "showModal" });
    const editor = document.createElement("dnn-rm-create-folder");
	logger.error(`showModal(): editor → ${editor}`, { "event.action": "showModal" });
    modal.appendChild(editor);
    document.body.appendChild(modal);
    await modal.show();
	logger.warn('<<< Exiting showModal()', { "event.action": "showModal" });
  }

  render() {
	logger.error('>>> Entering render()', { "event.action": "render" });
	logger.warn('<<< Exiting render()', { "event.action": "render" });
    return (
      <Host>
        <button onClick={() => void this.handleClick()}>
          <svg xmlns="http://www.w3.org/2000/svg" height="24px" viewBox="0 0 24 24" width="24px" fill="#000000"><path d="M0 0h24v24H0V0z" fill="none"/><path d="M20 6h-8l-2-2H4c-1.11 0-1.99.89-1.99 2L2 18c0 1.11.89 2 2 2h16c1.11 0 2-.89 2-2V8c0-1.11-.89-2-2-2zm-1 8h-3v3h-2v-3h-3v-2h3V9h2v3h3v2z"/></svg>
          <span>{state.localization.AddFolder}</span>
        </button>
      </Host>
    );
  }
}
