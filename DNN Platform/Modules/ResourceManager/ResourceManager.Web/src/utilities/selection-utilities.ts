import {Item} from "../services/ItemsClient";
import state from "../store/store";
const logger = console;

/** Various utilities that help with current selection of items. */
class SelectionUtilities{

  /** Check if an item is currently selected */
  public isItemSelected(item: Item)
  {
	logger.debug(`>>> Entering isItemSelected(item=${item})`);
	logger.debug(`<<< Exiting isItemSelected(item=${item})`);
    return state.selectedItems && state.selectedItems.includes(item);
  }

  /** Toggles the selection of an item. */
  public toggleItemSelected(item: Item): void {
    if (this.isItemSelected(item)){
	logger.debug(`>>> Entering toggleItemSelected(item=${item})`);
      state.selectedItems = state.selectedItems.filter(i => i != item);
    }
    else{
      state.selectedItems = [...state.selectedItems, item];
	logger.debug(`toggleItemSelected(item=${item}): state.selectedItems → ${state.selectedItems}`);
    }
	logger.debug(`<<< Exiting toggleItemSelected(item=${item})`);
  }
}

export const selectionUtilities = new SelectionUtilities();