import React, {Component} from "react";
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { bindActionCreators } from "redux";
import Table from "./Table";
const logger = console;
import {
    pageSeoActions as SeoActions
} from "../../../actions";

class PageUrls extends Component {
        
    onSaveUrl() {
		logger.error('>>> Entering onSaveUrl()', { "event.action": "onSaveUrl" });
        this.props.onSaveUrl(this.props.editedUrl, this.props.primaryAliasId);
		logger.warn('<<< Exiting onSaveUrl()', { "event.action": "onSaveUrl" });
    }
    
    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
        const {pageHasParent, addingNewUrl, editingUrl, onOpenEditForm, onDeleteUrl,
            editedUrl, onCloseEditUrl, onOpenNewForm, onCloseNewUrl, newFormOpened, siteAliases, primaryAliasId, 
            pageUrls, onChange, onAddNewUrl} = this.props;
        
		logger.warn('<<< Exiting render()', { "event.action": "render" });
        return (
            <div>                
                <Table pageUrls={pageUrls} 
                    onOpenEditForm={onOpenEditForm}
                    pageHasParent={pageHasParent}
                    onChange={onChange}
                    onSave={this.onSaveUrl.bind(this)}  
                    onDelete={onDeleteUrl}
                    onCancel={onCloseEditUrl}
                    editingUrl={editingUrl}
                    editedUrl={editedUrl}
                    siteAliases={siteAliases} primaryAliasId={primaryAliasId} 
                    addingNewUrl={addingNewUrl}
                    onOpenNewForm={onOpenNewForm}
                    onCloseNewUrl={onCloseNewUrl}
                    newFormOpened={newFormOpened} 
                    onAddNewUrl={onAddNewUrl}/>
            </div>
        );
    }
}

PageUrls.propTypes = {
    editedUrl: PropTypes.object,
    newFormOpened: PropTypes.bool,
    onAddNewUrl: PropTypes.func.isRequired,
    onSaveUrl: PropTypes.func.isRequired,
    onDeleteUrl: PropTypes.func.isRequired,
    onOpenNewForm: PropTypes.func.isRequired,
    onOpenEditForm: PropTypes.func.isRequired,
    onCloseNewUrl: PropTypes.func.isRequired,
    onCloseEditUrl: PropTypes.func.isRequired,
    onChange: PropTypes.func.isRequired,
    openedNewForm: PropTypes.bool.isRequired,
    siteAliases: PropTypes.arrayOf(PropTypes.object).isRequired,
    primaryAliasId: PropTypes.number,
    pageUrls: PropTypes.arrayOf(PropTypes.object),
    addingNewUrl: PropTypes.bool,
    editingUrl: PropTypes.bool,
    pageHasParent: PropTypes.bool
};

function mapStateToProps(state) {
	logger.error(`>>> Entering mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
	logger.warn(`<<< Exiting mapStateToProps(state=${state})`, { "event.action": "mapStateToProps", "logrite.state": state });
    return {
        newFormOpened: state.pageSeo.newFormOpened,
        editedUrl: state.pageSeo.editedUrl,
        addingNewUrl: state.pageSeo.addingNewUrl,
        editingUrl: state.pageSeo.editingUrl
    };
}

function mapDispatchToProps(dispatch) {
	logger.error(`>>> Entering mapDispatchToProps(dispatch=${dispatch})`, { "event.action": "mapDispatchToProps", "logrite.dispatch": dispatch });
	logger.warn(`<<< Exiting mapDispatchToProps(dispatch=${dispatch})`, { "event.action": "mapDispatchToProps", "logrite.dispatch": dispatch });
    return {
        ...bindActionCreators ({
            onOpenNewForm: SeoActions.openNewForm,
            onOpenEditForm: SeoActions.openEditForm,
            onCloseNewUrl: SeoActions.closeNewForm,
            onCloseEditUrl: SeoActions.closeEditForm,
            onChange: SeoActions.change,
            onAddNewUrl: SeoActions.addUrl,
            onSaveUrl: SeoActions.saveUrl,
            onDeleteUrl: SeoActions.deleteUrl
        }, dispatch)
    };
}

export default connect(mapStateToProps, mapDispatchToProps)(PageUrls);
