import React, { Component } from "react";
import PropTypes from "prop-types";
const logger = console;

class LazyLoad extends Component {
    constructor(props) {
        super(props);
		logger.debug(`>>> Entering constructor(props=${props})`);
        this.addListenerToFindBottom();
        this.state = {
            page: props.pageIndex
        };
    }

    componentDidUpdate(prevProps) {
		logger.debug(`>>> Entering componentDidUpdate(prevProps=${prevProps})`);
        if (this.props.filtersUpdated !== prevProps.filtersUpdated) {
            this.setState({
                page:0
            });
			logger.debug(`<<< Exiting componentDidUpdate(prevProps=${prevProps})`);
        }        
    }

    addListenerToFindBottom() {
		logger.debug('>>> Entering addListenerToFindBottom()');
        document.onscroll =  () => {
            // Internet Explorer
            let scrollY = window.scrollY ? window.scrollY : document.documentElement.scrollTop;
            
            let pos = scrollY + window.innerHeight; 
            if (pos === document.documentElement.scrollHeight) {
                if (this.props.hasMore) {
                    this.setState({
                        page:this.state.page+1
                    }, ()=>{
                        this.props.loadMore(this.state.page);
                    });
					logger.debug('<<< Exiting addListenerToFindBottom()');
                }
            }
        }; 
    }

    render() {
		logger.debug('>>> Entering render()');
		logger.debug('<<< Exiting render()');
        return (
            <div>
                <div>{this.props.children}</div>
                {this.props.hasMore?<div style={{textAlign:"center"}}>{this.props.loadingComponent}</div>:null}
            </div>
        );
    }
}

LazyLoad.propTypes = {
    pageIndex: PropTypes.number.isRequired,
    loadMore: PropTypes.func.isRequired,
    hasMore: PropTypes.bool.isRequired,
    useWindow: PropTypes.bool,
    children: PropTypes.node.isRequired,
    loadingComponent: PropTypes.node,
    filtersUpdated: PropTypes.bool
};

export default LazyLoad;
