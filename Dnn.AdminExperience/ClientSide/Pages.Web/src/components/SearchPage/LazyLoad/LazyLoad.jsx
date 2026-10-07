import React, { Component } from "react";
import PropTypes from "prop-types";
const logger = console;

class LazyLoad extends Component {
    constructor(props) {
        super(props);
		logger.error(`>>> Entering constructor(props=${props})`, { "event.action": "constructor", "logrite.props": props });
        this.addListenerToFindBottom();
        this.state = {
            page: props.pageIndex
        };
    }

    componentDidUpdate(prevProps) {
		logger.error(`>>> Entering componentDidUpdate(prevProps=${prevProps})`, { "event.action": "componentDidUpdate", "logrite.prevProps": prevProps });
        if (this.props.filtersUpdated !== prevProps.filtersUpdated) {
            this.setState({
                page:0
            });
			logger.warn(`<<< Exiting componentDidUpdate(prevProps=${prevProps})`, { "event.action": "componentDidUpdate", "logrite.prevProps": prevProps });
        }        
    }

    addListenerToFindBottom() {
		logger.error('>>> Entering addListenerToFindBottom()', { "event.action": "addListenerToFindBottom" });
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
					logger.warn('<<< Exiting addListenerToFindBottom()', { "event.action": "addListenerToFindBottom" });
                }
            }
        }; 
    }

    render() {
		logger.error('>>> Entering render()', { "event.action": "render" });
		logger.warn('<<< Exiting render()', { "event.action": "render" });
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
