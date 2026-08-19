import PropTypes from "prop-types";
import React from "react";
import Resx from "../localization";
const logger = console;
import { Button } from "@dnnsoftware/dnn-react-common";

export default class NewSiteGroup extends React.Component {
    constructor(props) {
        super(props);
		logger.debug(`>>> Entering constructor(props=${props})`);
        this.state = {
            siteId: null
        };
		logger.debug(`<<< Exiting constructor(props=${props})`);
    }

    onNew() {
		logger.debug('>>> Entering onNew()');
        this.props.onNewGroup(this.state.siteId);
        this.setState({
            siteId: null
        });
		logger.debug('<<< Exiting onNew()');
    }

    render() {
		logger.debug('>>> Entering render()');
        if (this.props.disabled && this.state.sitedId)
            this.setState({ siteId: null });
		logger.debug('<<< Exiting render()');
        return (
            <div style={{ float: "right" }}>
                <select
                    name="sites"
                    style={{
                        padding: "12px",
                        marginRight: "12px"
                    }}
                    onChange={e =>
                        this.setState({
                            siteId:
                e.target.value === "NoSiteSelected" ? null : e.target.value
                        })
                    }
                >
                    <option value="NoSiteSelected">
                        {Resx.get("ChooseASite.Label")}
                    </option>
                    {(this.props.unassignedSites || []).map(site => (
                        <option key={site.PortalId.toString()} value={site.PortalId}>
                            {site.PortalName}
                        </option>
                    ))}
                </select>
                <Button
                    type="primary"
                    size="large"
                    disabled={!this.state.siteId || this.props.disabled}
                    onClick={() => this.onNew()}
                >
                    {Resx.get("NewSiteGroup.Button")}
                </Button>
            </div>
        );
    }
}

NewSiteGroup.propTypes = {
    unassignedSites: PropTypes.array,
    onNewGroup: PropTypes.func,
    disabled: PropTypes.bool
};
