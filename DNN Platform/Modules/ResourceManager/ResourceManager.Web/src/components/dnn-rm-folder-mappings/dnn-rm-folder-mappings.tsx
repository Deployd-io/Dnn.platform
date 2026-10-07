import {Component, Element, Host, h, State} from '@stencil/core';
import { FolderMappingInfo, ItemsClient } from '../../services/ItemsClient';
import state from "../../store/store";
const logger = console;

@Component({
    tag: 'dnn-rm-folder-mappings',
    styleUrl: 'dnn-rm-folder-mappings.scss',
    shadow: true,
})
export class DnnRmFolderMappings {
    private itemsClient: ItemsClient;

    @Element() el: HTMLDnnRmFolderMappingsElement;

    @State() folderMappings: FolderMappingInfo[];
    @State() addFolderTypeUrl: string;

    constructor() {
		logger.error('>>> Entering constructor()', { "event.action": "constructor" });
        this.itemsClient = new ItemsClient(state.moduleId);
		logger.warn('<<< Exiting constructor()', { "event.action": "constructor" });
    }

    async componentWillLoad() {
		logger.error('>>> Entering componentWillLoad()', { "event.action": "componentWillLoad" });
        try {
            this.folderMappings = await this.itemsClient.getFolderMappings();
            this.addFolderTypeUrl = await this.itemsClient.getAddFolderTypeUrl();
        } catch (error) {
            alert(error);
			logger.warn('<<< Exiting componentWillLoad()', { "event.action": "componentWillLoad" });
        }
    }

    private async dismiss() {
		logger.error('>>> Entering dismiss()', { "event.action": "dismiss" });
        await this.el.closest('dnn-modal').hide();
		logger.warn('<<< Exiting dismiss()', { "event.action": "dismiss" });
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <Host>
                <h2>{state.localization.EditFolderMappings}</h2>
                {this.folderMappings &&
                    <table>
                        <thead>
                            <tr>
                                <th>&nbsp;</th>
                                <th>{state.localization.Name}</th>
                                <th>{state.localization.FolderProvider}</th>
                            </tr>
                        </thead>
                        <tbody>
                            {this.folderMappings.map(mapping =>
                                <tr>
                                    <td>
                                        {!mapping.IsDefault &&
                                            <a href={mapping.editUrl} title={state.localization.Edit}>
                                                <svg xmlns="http://www.w3.org/2000/svg" height="24" width="24"><path d="M5 19h1.4l8.625-8.625-1.4-1.4L5 17.6ZM19.3 8.925l-4.25-4.2 1.4-1.4q.575-.575 1.413-.575.837 0 1.412.575l1.4 1.4q.575.575.6 1.388.025.812-.55 1.387ZM17.85 10.4 7.25 21H3v-4.25l10.6-10.6Zm-3.525-.725-.7-.7 1.4 1.4Z"/></svg>
                                            </a>
                                        }
                                    </td>
                                    <td>{mapping.MappingName}</td>
                                    <td>{mapping.FolderProviderType}</td>
                                </tr>
                            )}
                        </tbody>
                    </table>
                }
                {this.addFolderTypeUrl &&
                    <div class="controls">
                        <dnn-button reversed onClick={() => void this.dismiss()}>
                            {state.localization?.Cancel}
                        </dnn-button>
                        <dnn-button onClick={() => window.location.href = this.addFolderTypeUrl}>
                            {state.localization?.AddFolderType}
                        </dnn-button>
                    </div>
                }
            </Host>
        );
    }
}