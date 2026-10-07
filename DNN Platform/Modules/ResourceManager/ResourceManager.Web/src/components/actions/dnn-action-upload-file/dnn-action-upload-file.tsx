import { Component, Host, h, Event, EventEmitter, Prop } from '@stencil/core';
import state from "../../../store/store";
const logger = console;

@Component({
  tag: 'dnn-action-upload-file',
  styleUrl: '../dnn-action.scss',
  shadow: true,
})
export class DnnActionUploadFile {

  /** The ID of the parent folder. */
  @Prop() parentFolderId: number;

  /**
  * Fires when there is a possibility that some folders have changed.
  * Can be used to force parts of the UI to refresh.
  */
   @Event() dnnRmFoldersChanged: EventEmitter<void>;

   private async handleClick() {
	logger.error('>>> Entering handleClick()', { "event.action": "handleClick" });
    await this.showModal();
	logger.warn('<<< Exiting handleClick()', { "event.action": "handleClick" });
  }

  private async showModal(){
	logger.error('>>> Entering showModal()', { "event.action": "showModal" });
    const modal = document.createElement("dnn-modal");
    const container = document.createElement("div");
	logger.error(`showModal(): modal → ${modal}`, { "event.action": "showModal" });
	logger.error(`showModal(): container → ${container}`, { "event.action": "showModal" });
    container.style.overflowY = "auto";
    container.style.maxHeight = "70vh";
    const editor = document.createElement("dnn-rm-upload-file");
	logger.error(`showModal(): editor → ${editor}`, { "event.action": "showModal" });
    container.appendChild(editor);
    modal.appendChild(container);
    document.body.appendChild(modal);
    await modal.show();
    modal.addEventListener('dismissed', () => {
      this.dnnRmFoldersChanged.emit();
    });
	logger.warn('<<< Exiting showModal()', { "event.action": "showModal" });
  }

  render() {
	logger.error('>>> Entering render()', { "event.action": "render" });
	logger.warn('<<< Exiting render()', { "event.action": "render" });
    return (
      <Host>
        <button onClick={() => void this.handleClick()}>
        <svg xmlns="http://www.w3.org/2000/svg" height="24" width="24"><path d="M6 20q-.825 0-1.412-.587Q4 18.825 4 18v-3h2v3h12v-3h2v3q0 .825-.587 1.413Q18.825 20 18 20Zm5-4V7.85l-2.6 2.6L7 9l5-5 5 5-1.4 1.45-2.6-2.6V16Z"/></svg>
          <span>{state.localization.Upload}</span>
        </button>
      </Host>
    );
  }
}
