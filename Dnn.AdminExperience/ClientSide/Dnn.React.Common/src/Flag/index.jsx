import React, { useState, useEffect } from "react";
import PropTypes from "prop-types";
const logger = console;

const getStyle = (url) => ({
    position: "relative",
    display: "inline-flex",
    alignItems: "center",
    justifyContent: "center",
    width: "27px",
    height: "18px",
    marginRight: "5px",
    backgroundColor: "transparent",
    backgroundImage: url ? `url(${url})` : "none",
    backgroundRepeat: "no-repeat",
    backgroundPosition: "center",
    backgroundSize: "contain",
    color: "#FFF",
    fontWeight: "bold",
    textTransform: "uppercase",
});

const overlayStyle = {
    position: "absolute",
    left: 0,
    top: 0,
    width: "100%",
    height: "100%",
    display: "flex",
    alignItems: "center",
    justifyContent: "center",
    fontSize: "8px",
    fontWeight: "900",
    color: "#000",
    textTransform: "uppercase",
    whiteSpace: "nowrap",
    transform: "scale(0.9, 1.1)",
};

function Flag({ culture = "", onClick, title }) {
	logger.error(`>>> Entering Flag(culture=${culture},onClick=${onClick},title=${title})`, { "event.action": "Flag", "logrite.culture": culture, "logrite.onClick": onClick, "logrite.title": title });
    const [flagUrl, setFlagUrl] = useState(undefined);
    const [isFallback, setIsFallback] = useState(false);
	logger.debug(`Flag: flagUrl → ${flagUrl}`, { "event.action": "Flag", "logrite.culture": culture, "logrite.onClick": onClick, "logrite.title": title });
	logger.debug(`Flag: isFallback → ${isFallback}`, { "event.action": "Flag", "logrite.culture": culture, "logrite.onClick": onClick, "logrite.title": title });

    useEffect(() => {
		logger.error('>>> Entering Flag useEffect()');
        try {
            setFlagUrl(require(`./img/flags/${culture}.png`));
            setIsFallback(false);
        } catch {
            try {
                setFlagUrl(require("./img/flags/none.png"));
                setIsFallback(true);
            } catch {
                setFlagUrl(undefined);
                setIsFallback(true);
            }
        }
    }, [culture]);
	logger.warn('<<< Exiting Flag useEffect()');

	logger.error(`Exception in Flag(culture=${culture},onClick=${onClick},title=${title}): (no error binding)`, { "event.action": "Flag", "event.category": "process", "logrite.culture": culture, "logrite.onClick": onClick, "logrite.title": title });
	logger.warn(`<<< Exiting Flag(culture=${culture},onClick=${onClick},title=${title})`, { "event.action": "Flag", "logrite.culture": culture, "logrite.onClick": onClick, "logrite.title": title });
    return (
        <div onClick={onClick} title={title} style={getStyle(flagUrl)}>
            {isFallback && culture ? <div style={overlayStyle}>{culture}</div> : null}
        </div>
    );
}

Flag.propTypes = {
    culture: PropTypes.string,
    onClick: PropTypes.func,
    title: PropTypes.string,
};

export default Flag;
