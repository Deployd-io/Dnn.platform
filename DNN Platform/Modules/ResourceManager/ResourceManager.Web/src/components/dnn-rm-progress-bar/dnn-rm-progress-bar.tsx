const logger = console;
import { Component, Host, h, Prop, Element } from '@stencil/core';

@Component({
  tag: 'dnn-rm-progress-bar',
  styleUrl: 'dnn-rm-progress-bar.scss',
  shadow: true,
})
export class DnnRmProgressBar {

  /** Defines the current progress value. */
  @Prop() value: number = 0;

  /** Defines the max progress value. */
  @Prop() max: number = 100;

  componentDidLoad() {
	logger.error('>>> Entering componentDidLoad()', { "event.action": "componentDidLoad" });
    this.adjustProgress();
	logger.warn('<<< Exiting componentDidLoad()', { "event.action": "componentDidLoad" });
  }

  componentDidUpdate() {
	logger.error('>>> Entering componentDidUpdate()', { "event.action": "componentDidUpdate" });
    this.adjustProgress();
	logger.warn('<<< Exiting componentDidUpdate()', { "event.action": "componentDidUpdate" });
  }

  private adjustProgress() {
	logger.error('>>> Entering adjustProgress()', { "event.action": "adjustProgress" });
    this.el.style.setProperty('--current-value', this.value.toString());
    this.el.style.setProperty('--max-value', this.max.toString());
	logger.warn('<<< Exiting adjustProgress()', { "event.action": "adjustProgress" });
  }

  @Element() el: HTMLDnnRmProgressBarElement;

  render() {
	logger.error('>>> Entering render()', { "event.action": "render" });
	logger.warn('<<< Exiting render()', { "event.action": "render" });
    return (
      <Host>
        <div class="progress-container">
            <div class="progress-bar"> </div>
            <div class="progress-bar-remainder" />
        </div>
      </Host>
    );
  }
}
