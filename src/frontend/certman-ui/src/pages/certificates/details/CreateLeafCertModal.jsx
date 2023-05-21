import { useState } from 'react';
import apiService from '../../../services/apiService';
import DnsNamesInput from "./DnsNamesInput";

function CreateLeafCertModal({ caCertId, onCreated }) {
    const [showModal, setShowModal] = useState(false);
    const [formData, setFormData] = useState({
        name: '',
        password: '',
        country: 'BE',
        state: 'Flanders',
        locality: 'Tienen',
        organization: 'Heapzilla',
        organizationUnit: 'Rubenshof20',
        commonName: '',
        dnsNames: []
    });

    const handleChange = (event) => {
        setFormData({
            ...formData,
            [event.target.name]: event.target.value
        });
    };

    const handleSubmit = (event) => {
        console.log('handleSubmit');
        event.preventDefault();
        apiService.createLeafCert(caCertId, formData)
            .then(() => {
                onCreated();
                setShowModal(false);
            })
            .catch(error => console.error('Error:', error));
    };

    return (
        <div>
            <button className="button is-success is-small" onClick={() => setShowModal(true)}>Create Leaf Certificate
            </button>
            <div className={`modal ${showModal ? 'is-active' : ''}`}>
                <div className="modal-background"></div>
                <div className="modal-content">
                    <header className="modal-card-head">
                        <p className="modal-card-title">Create New Leaf Certificate</p>
                        <button className="delete" aria-label="close" onClick={() => setShowModal(false)}></button>
                    </header>

                    <section className="modal-card-body">
                        <form onSubmit={handleSubmit}>
                            <div className="field">
                                <label className="label is-small">Name</label>
                                <div className="control">
                                    <input className="input is-small" type="text" name="name" onChange={handleChange} value={formData.name} placeholder="Name" required/>
                                </div>
                            </div>

                            <div className="field">
                                <label className="label is-small">Password</label>
                                <div className="control">
                                    <input className="input is-small" type="password" name="password" onChange={handleChange} value={formData.password} placeholder="Password" required/>
                                </div>
                            </div>

                            <div className="field">
                                <label className="label is-small">Common Name</label>
                                <div className="control">
                                    <input className="input is-small" type="text" name="commonName" onChange={handleChange} value={formData.commonName} placeholder="Common Name" required/>
                                </div>
                            </div>

                            {/*country*/}
                            <div className="field">
                                <label className="label is-small">Country</label>
                                <div className="control">
                                    <input className="input is-small" type="text" name="country" onChange={handleChange} value={formData.country} placeholder="Country"/>
                                </div>
                            </div>

                            {/*state*/}
                            <div className="field">
                                <label className="label is-small">State</label>
                                <div className="control">
                                    <input className="input is-small" type="text" name="state" onChange={handleChange} value={formData.state} placeholder="State"/>
                                </div>
                            </div>

                            {/*locality*/}
                            <div className="field">
                                <label className="label is-small">Locality</label>
                                <div className="control">
                                    <input className="input is-small" type="text" name="locality" onChange={handleChange} value={formData.locality} placeholder="Locality"/>
                                </div>
                            </div>

                            {/*organization*/}
                            <div className="field">
                                <label className="label is-small">Organization</label>
                                <div className="control">
                                    <input className="input is-small" type="text" name="organization" onChange={handleChange} value={formData.organization} placeholder="Organization"/>
                                </div>
                            </div>

                            {/*organizationUnit*/}
                            <div className="field">
                                <label className="label is-small">Organization Unit</label>
                                <div className="control">
                                    <input className="input is-small" type="text" name="organizationUnit" onChange={handleChange} value={formData.organizationUnit} placeholder="Organization Unit"/>
                                </div>
                            </div>

                            <DnsNamesInput
                                onDnsNamesChange={(newDnsNames) => setFormData({...formData, dnsNames: newDnsNames})}/>
                        </form>
                    </section>

                    <footer className="modal-card-foot">
                        <button className="button is-success is-small" type="submit">Create</button>
                        <button className="button is-small" type="button" onClick={() => setShowModal(false)}>Cancel</button>
                    </footer>
                </div>
            </div>
        </div>
    );
}

export default CreateLeafCertModal;