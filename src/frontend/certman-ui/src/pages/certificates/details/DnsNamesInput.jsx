import { useState } from 'react';

function DnsNamesInput({ onDnsNamesChange }) {
    const [dnsNames, setDnsNames] = useState([]);
    const [currentName, setCurrentName] = useState('');

    const handleAdd = () => {
        if (currentName !== '' && !dnsNames.includes(currentName)) {
            const newDnsNames = [...dnsNames, currentName];
            setDnsNames(newDnsNames);
            setCurrentName('');
            onDnsNamesChange(newDnsNames);
        }
    };

    const handleRemove = (name) => {
        const newDnsNames = dnsNames.filter(dnsName => dnsName !== name);
        setDnsNames(newDnsNames);
        onDnsNamesChange(newDnsNames);
    };

    const handleInputChange = (e) => {
        setCurrentName(e.target.value);
    };

    return (
        <div className="field">
            <label className="label">DNS Names:</label>
            {dnsNames.map(name => (
                <div className="control" key={name}>
                    <div className="tags has-addons">
                        <span className="tag">{name}</span>
                        <a className="tag is-delete" onClick={() => handleRemove(name)}></a>
                    </div>
                </div>
            ))}
            <div className="field has-addons">
                <div className="control">
                    <input
                        className="input is-small"
                        type="text"
                        value={currentName}
                        onChange={handleInputChange}
                        placeholder="New DNS name"
                    />
                </div>
                <div className="control">
                    <button
                        className="button is-info is-small"
                        type="button"
                        onClick={handleAdd}
                    >
                        Add DNS Name
                    </button>
                </div>
            </div>
        </div>
    );
}

export default DnsNamesInput;