import { Component, Host, h, Prop } from '@stencil/core';
import { Item } from '../../../services/ItemsClient';
import state from "../../../store/store";
const logger = console;

@Component({
  tag: 'dnn-action-unlink-items',
  styleUrl: '../dnn-action.scss',
  shadow: true,
})
export class DnnActionUnlinkItems {

  /** The list of items selected. */
  @Prop() items!: Item[];

  private async handleClick() {
	logger.error('>>> Entering handleClick()', { "event.action": "handleClick" });
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
    const editor = document.createElement("dnn-rm-unlink-items");
	logger.error(`showModal(): editor → ${editor}`, { "event.action": "showModal" });
    editor.items = this.items;
    modal.appendChild(editor);
	logger.debug(`showModal(): editor.items → ${editor.items}`, { "event.action": "showModal" });
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
        <svg xmlns="http://www.w3.org/2000/svg" height="24px" viewBox="0 0 24 24" width="24px" fill="#000000"><path d="M0 0h24v24H0V0z" fill="none"/><path d="M14.39 11L16 12.61V11zM17 7h-4v1.9h4c1.71 0 3.1 1.39 3.1 3.1 0 1.27-.77 2.37-1.87 2.84l1.4 1.4C21.05 15.36 22 13.79 22 12c0-2.76-2.24-5-5-5zM2 4.27l3.11 3.11C3.29 8.12 2 9.91 2 12c0 2.76 2.24 5 5 5h4v-1.9H7c-1.71 0-3.1-1.39-3.1-3.1 0-1.59 1.21-2.9 2.76-3.07L8.73 11H8v2h2.73L13 15.27V17h1.73l4.01 4.01 1.41-1.41L3.41 2.86 2 4.27z"/></svg>
          <span>{state.localization.Unlink}</span>
        </button>
      </Host>
    );
  }
}
