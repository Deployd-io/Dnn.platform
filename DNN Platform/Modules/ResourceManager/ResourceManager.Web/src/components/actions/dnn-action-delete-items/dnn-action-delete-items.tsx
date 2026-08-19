import { Component, Host, h, Prop } from '@stencil/core';
import { Item } from '../../../services/ItemsClient';
import state from "../../../store/store";
const logger = console;

@Component({
  tag: 'dnn-action-delete-items',
  styleUrl: '../dnn-action.scss',
  shadow: true,
})
export class DnnActionDeleteItems {

  /** The list of items to delete. */
  @Prop() items!: Item[];

  private async handleClick() {
	logger.debug('>>> Entering handleClick()');
    await this.showModal();
	logger.debug('<<< Exiting handleClick()');
  }

  private async showModal(){
	logger.debug('>>> Entering showModal()');
    const modal = document.createElement("dnn-modal");
    modal.preventBackdropDismiss = true;
	logger.debug(`showModal(): modal → ${modal}`);
	logger.debug(`showModal(): modal.preventBackdropDismiss → ${modal.preventBackdropDismiss}`);
    modal.hideCloseButton = true;
	logger.debug(`showModal(): modal.hideCloseButton → ${modal.hideCloseButton}`);
    const editor = document.createElement("dnn-rm-delete-items");
	logger.debug(`showModal(): editor → ${editor}`);
    editor.items = this.items;
    modal.appendChild(editor);
	logger.debug(`showModal(): editor.items → ${editor.items}`);
    document.body.appendChild(modal);
    await modal.show();
	logger.debug('<<< Exiting showModal()');
  }

  render() {
	logger.debug('>>> Entering render()');
	logger.debug('<<< Exiting render()');
    return (
      <Host>
        <button onClick={() => void this.handleClick()}>
          <svg xmlns="http://www.w3.org/2000/svg" height="24px" viewBox="0 0 24 24" width="24px" fill="#000000"><path d="M0 0h24v24H0V0z" fill="none"/><path d="M16 9v10H8V9h8m-1.5-6h-5l-1 1H5v2h14V4h-3.5l-1-1zM18 7H6v12c0 1.1.9 2 2 2h8c1.1 0 2-.9 2-2V7z"/></svg>
          <span>{state.localization.Delete}</span>
        </button>
      </Host>
    );
  }
}
