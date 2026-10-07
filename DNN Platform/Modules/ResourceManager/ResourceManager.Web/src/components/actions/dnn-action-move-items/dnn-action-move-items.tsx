import { Component, Host, h, Prop } from '@stencil/core';
import { Item } from '../../../services/ItemsClient';
import state from "../../../store/store";
const logger = console;

@Component({
  tag: 'dnn-action-move-items',
  styleUrl: '../dnn-action.scss',
  shadow: true,
})
export class DnnActionMoveItems {

  /** The list of items to move. */
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
    const editor = document.createElement("dnn-rm-move-items");
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
        <svg xmlns="http://www.w3.org/2000/svg" enable-background="new 0 0 24 24" height="24px" viewBox="0 0 24 24" width="24px" fill="#000000"><g><rect fill="none" height="24" width="24"/></g><g><path d="M20,6h-8l-2-2H4C2.9,4,2,4.9,2,6v12c0,1.1,0.9,2,2,2h16c1.1,0,2-0.9,2-2V8C22,6.9,21.1,6,20,6z M20,18H4V6h5.17l1.41,1.41 L11.17,8H20V18z M12.16,12H8v2h4.16l-1.59,1.59L11.99,17L16,13.01L11.99,9l-1.41,1.41L12.16,12z"/></g></svg>
          <span>{state.localization.Move}</span>
        </button>
      </Host>
    );
  }
}
