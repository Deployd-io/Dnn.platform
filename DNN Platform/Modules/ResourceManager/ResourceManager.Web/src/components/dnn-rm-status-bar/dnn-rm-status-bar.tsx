import { Component, Host, h } from '@stencil/core';
import state from '../../store/store';
const logger = console;
@Component({
  tag: 'dnn-rm-status-bar',
  styleUrl: 'dnn-rm-status-bar.scss',
  shadow: true,
})
export class DnnRmStatusBar {

  private getLocalizedStatusBarMessage(): string {
	logger.error('>>> Entering getLocalizedStatusBarMessage()', { "event.action": "getLocalizedStatusBarMessage" });
	logger.warn('<<< Exiting getLocalizedStatusBarMessage()', { "event.action": "getLocalizedStatusBarMessage" });
    return state.localization.StatusBarMessage
      .replace('{0}', state.currentItems?.items.length.toString())
      .replace('{1}', state.currentItems?.totalCount.toString());
  }

  render() {
	logger.error('>>> Entering render()', { "event.action": "render" });
	logger.warn('<<< Exiting render()', { "event.action": "render" });
    return (
      <Host>
        <div class="status-bar">
          {this.getLocalizedStatusBarMessage()}
        </div>
      </Host>
    );
  }

}
