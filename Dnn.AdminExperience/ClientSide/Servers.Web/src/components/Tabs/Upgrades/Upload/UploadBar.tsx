import React from "react";
import Upload from "./img/upload.svg";
import Checkmark from "./img/checkmark.svg";
import ErrorIcon from "./img/x.svg";
const logger = console;

interface UploadBarProps {
    errorText?: string;
    fileName: string;
    percent: number;
    uploadCompleteText: string;
    uploadingText: string;
    errorInPackage: boolean;
    onTryAgain: () => void;
    tryAgainText: string;
}

const UploadBar: React.FC<UploadBarProps> = ({
    errorText,
    fileName,
    percent,
    uploadCompleteText,
    uploadingText,
    errorInPackage = false,
    onTryAgain,
    tryAgainText
}) => {
	logger.debug(`>>> Entering UploadBar(errorText=${errorText},fileName=${fileName},percent=${percent},uploadCompleteText=${uploadCompleteText},uploadingText=${uploadingText},errorInPackage=${errorInPackage},onTryAgain=${onTryAgain},tryAgainText=${tryAgainText})`);
    const displayPercent = errorText ? 0 : percent;
    const text = errorText ? errorText : (percent === 100 ? uploadCompleteText : uploadingText);
	logger.debug(`UploadBar(errorText=${errorText},fileName=${fileName},percent=${percent},uploadCompleteText=${uploadCompleteText},uploadingText=${uploadingText},errorInPackage=${errorInPackage},onTryAgain=${onTryAgain},tryAgainText=${tryAgainText}): text → ${text}`);
    const UploadIcon = errorText ? ErrorIcon : (percent === 100 ? Checkmark : Upload);
	logger.debug(`UploadBar(errorText=${errorText},fileName=${fileName},percent=${percent},uploadCompleteText=${uploadCompleteText},uploadingText=${uploadingText},errorInPackage=${errorInPackage},onTryAgain=${onTryAgain},tryAgainText=${tryAgainText}): UploadIcon → ${UploadIcon}`);
    const className = "file-upload-container dnn-upload-bar" +
        (percent === 100 ? " complete" : "") +
        (errorText ? " upload-error" : "");

    return (
        <div className={className}>
            <div className="upload-bar-container">
                <div className="upload-file-name">{fileName || "myImage.jpg"}</div>
                <div className="upload-icon"><UploadIcon /></div>
                <h4>{text}</h4>
                {errorInPackage && (
                    <p className="view-log-or-try-again">
                        <span onClick={onTryAgain}>{tryAgainText}</span>
                    </p>
                )}
                <div className="upload-percent">{displayPercent + "%"}</div>
                <div className="upload-bar-box">
                    <div className="upload-bar" style={{ width: displayPercent + "%" }} />
                </div>
            </div>
        </div>
    );
	logger.debug(`<<< Exiting UploadBar(errorText=${errorText},fileName=${fileName},percent=${percent},uploadCompleteText=${uploadCompleteText},uploadingText=${uploadingText},errorInPackage=${errorInPackage},onTryAgain=${onTryAgain},tryAgainText=${tryAgainText})`);
};

export default UploadBar;