import React from "react";
import PropTypes from "prop-types";
const logger = console;
const maxItems = 4;

const Breadcrumbs = ({ items, onSelectedItem }) => {
logger.debug(`>>> Entering Breadcrumbs(items=${items},onSelectedItem=${onSelectedItem})`);

    function onClick(tabId) {
		logger.debug(`>>> Entering onClick(tabId=${tabId})`);
        if (tabId)
            onSelectedItem(tabId);
			logger.debug(`<<< Exiting onClick(tabId=${tabId})`);
    }

	logger.debug(`<<< Exiting Breadcrumbs(items=${items},onSelectedItem=${onSelectedItem})`);
    return (
        <div className="breadcrumbs-container">
            {items.length > maxItems &&
                <div>
                    <span className="more"
                        title={items.map(i => i.name).join(" > ")}
                        onClick={() => onClick(0)}>&nbsp;</span>
                </div>}

            {items.slice(Math.max(items.length - maxItems, 0)).map((item) => {

                return (
                    <div key={item.tabId} onClick={() => onClick(item.tabId)}>
                        <span>{item.name}</span>
                    </div>);
            })
            }
        </div>
    );
};

Breadcrumbs.propTypes = {
    items: PropTypes.array.isRequired,
    onSelectedItem: PropTypes.func.isRequired
};

export default Breadcrumbs;