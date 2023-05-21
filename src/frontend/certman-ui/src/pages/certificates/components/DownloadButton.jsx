import { useCallback } from 'react';

function DownloadButton({ certId, fileName, apiMethod }) {
    const downloadFile = useCallback(async () => {
        const fileBlob = await apiMethod(certId);
        const url = window.URL.createObjectURL(fileBlob);
        const link = document.createElement('a');
        link.href = url;
        link.setAttribute('download', fileName);
        document.body.appendChild(link);
        link.click();
        link.remove();
    }, [certId, fileName, apiMethod]);

    return (
        <button className="button is-small is-ghost" onClick={downloadFile}>
            <span className="icon">
                <i className="fa fa-download"></i>
            </span>
            <span>{fileName}</span>
        </button>
    );
}

export default DownloadButton;