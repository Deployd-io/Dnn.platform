import {Item} from "../services/ItemsClient";
import state from "../store/store";
const logger = console;

/** Various utilities that help with current selection of items. */
class SelectionUtilities{

  /** Check if an item is currently selected */
  public isItemSelected(item: Item)
  {
	logger.error(`>>> Entering isItemSelected(item=${item})`, { "event.action": "isItemSelected", "logrite.item": item });
	logger.warn(`<<< Exiting isItemSelected(item=${item})`, { "event.action": "isItemSelected", "logrite.item": item });
    return state.selectedItems && state.selectedItems.includes(item);
  }

  /** Toggles the selection of an item. */
  public toggleItemSelected(item: Item): void {
	logger.error(`>>> Entering toggleItemSelected(item=${item})`, { "event.action": "toggleItemSelected", "logrite.item": item });
    if (this.isItemSelected(item)){
      state.selectedItems = state.selectedItems.filter(i => i != item);
    }
    else{
      state.selectedItems = [...state.selectedItems, item];
	logger.warn(`<<< Exiting toggleItemSelected(item=${item})`, { "event.action": "toggleItemSelected", "logrite.item": item });
    }
  }
}

export const selectionUtilities = new SelectionUtilities();