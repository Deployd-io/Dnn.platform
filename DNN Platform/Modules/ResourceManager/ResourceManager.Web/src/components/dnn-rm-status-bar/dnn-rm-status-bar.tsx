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
	logger.debug('>>> Entering getLocalizedStatusBarMessage()');
	logger.debug('<<< Exiting getLocalizedStatusBarMessage()');
    return state.localization.StatusBarMessage
      .replace('{0}', state.currentItems?.items.length.toString())
      .replace('{1}', state.currentItems?.totalCount.toString());
  }

  render() {
	logger.debug('>>> Entering render()');
	logger.debug('<<< Exiting render()');
    return (
      <Host>
        <div class="status-bar">
          {this.getLocalizedStatusBarMessage()}
        </div>
      </Host>
    );
  }

}
