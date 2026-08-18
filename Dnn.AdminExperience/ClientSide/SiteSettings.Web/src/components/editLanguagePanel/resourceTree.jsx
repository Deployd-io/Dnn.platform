import React, { Component } from "react";
import PropTypes from "prop-types";
import resx from "resources";
import { Scrollbars } from "react-custom-scrollbars";
import Folder from "./folder";
const logger = console;
import { GridCell, SvgIcons, Label, Collapsible } from "@dnnsoftware/dnn-react-common";
const parentTermTreeStyle = {
    width: "100%",
    height: "250px",
    border: "1px solid #BFBFBF",
    background: "white",
    boxSizing: "border-box"
};

 
function mapChildFolders(folders, getChildFolders, getResxEntries, resxBeingEdited) {
	logger.debug(`>>> Entering mapChildFolders(folders=${folders},getChildFolders=${getChildFolders},getResxEntries=${getResxEntries},resxBeingEdited=${resxBeingEdited})`);
	logger.debug(`<<< Exiting mapChildFolders(folders=${folders},getChildFolders=${getChildFolders},getResxEntries=${getResxEntries},resxBeingEdited=${resxBeingEdited})`);
    return folders.map((folder, i) => {
        const isResxFile = folder.NewValue.indexOf(".resx") !== -1;

        return <Folder
            onClick={(!isResxFile ? getChildFolders.bind(this) : getResxEntries.bind(this))}
            folder={folder}
            ChildFolders={folder.ChildFolders}
            isSelected={resxBeingEdited === folder.NewValue}
            key={i}>
            {(folder.ChildFolders && folder.ChildFolders.length > 0) && mapChildFolders(folder.ChildFolders, getChildFolders, getResxEntries, resxBeingEdited)}
        </Folder>;
    });
}

class ResourceTree extends Component {

    constructor() {
        super();
		logger.debug('>>> Entering constructor()');
        this.state = {
            treeOpened: false
        };
        this.handleClick = this.handleClick.bind(this);
		logger.debug(`constructor(): this.state → ${this.state}`);
        this.node = React.createRef();
		logger.debug('<<< Exiting constructor()');
    }

    componentDidMount() {
		logger.debug('>>> Entering componentDidMount()');
        document.addEventListener("click", this.handleClick, false);
        this._isMounted = true;
		logger.debug(`componentDidMount(): this._isMounted → ${this._isMounted}`);
		logger.debug('<<< Exiting componentDidMount()');
    }

    componentWillUnmount() {
		logger.debug('>>> Entering componentWillUnmount()');
        document.removeEventListener("click", this.handleClick, false);
        this._isMounted = false;
		logger.debug(`componentWillUnmount(): this._isMounted → ${this._isMounted}`);
		logger.debug('<<< Exiting componentWillUnmount()');
    }

    handleClick(e) {
		logger.debug(`>>> Entering handleClick(e=${e})`);
        if (!this._isMounted) { return; }
        const node = this.node.current;
        if (node && node.contains(e.target)) {
            return;
        }
        this.hide();
		logger.debug(`<<< Exiting handleClick(e=${e})`);
    }
    hide() {
		logger.debug('>>> Entering hide()');
        this.setState({ treeOpened: false });
		logger.debug('<<< Exiting hide()');
    }
    onToggleTree() {
		logger.debug('>>> Entering onToggleTree()');
        this.setState({
            treeOpened: !this.state.treeOpened
        });
		logger.debug('<<< Exiting onToggleTree()');
    }
    render() {
		logger.debug('>>> Entering render()');
        const {
            languageFolders,
            getChildFolders,
            getResxEntries,
            resxBeingEdited,
            resxBeingEditedDisplay
        } = this.props;
		logger.debug('<<< Exiting render()');
        return (
            <div ref={this.node}>
                <GridCell columnSize={100} className="resource-file-tree-container">
                    <Label label={resx.get("ResourceFile")} />
                    <div className="resource-file-dropdown" onClick={this.onToggleTree.bind(this)} style={{ width: "50%" }}>
                        {resxBeingEditedDisplay || resx.get("SelectResourcePlaceholder")}
                        <div className="dropdown-icon"><SvgIcons.ArrowDownIcon /></div>
                    </div>
                    <Collapsible isOpened={this.state.treeOpened} className="tree-container">
                        <Scrollbars style={parentTermTreeStyle}>
                            <ul className="resource-tree root-level parent-tree">
                                {mapChildFolders(languageFolders, getChildFolders, getResxEntries, resxBeingEdited)}
                            </ul>
                        </Scrollbars>
                    </Collapsible>
                </GridCell>
            </div>
        );
    }
}

ResourceTree.propTypes = {
    languageBeingEdited: PropTypes.object,
    ModeOptions: PropTypes.array,
    languageFolders: PropTypes.array,
    languageFiles: PropTypes.array,
    getChildFolders: PropTypes.func,
    getResxEntries: PropTypes.func,
    resxBeingEdited: PropTypes.string,
    onToggleTree: PropTypes.func,
    resxBeingEditedDisplay: PropTypes.string
};

export default ResourceTree;