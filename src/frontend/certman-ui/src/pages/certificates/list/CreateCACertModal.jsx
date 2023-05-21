// CreateCertModal.jsx
import React, {useState} from 'react';
import apiService from '../../../services/apiService';

const CreateCACertModal = ({onCertCreated}) => {
    const [showModal, setShowModal] = useState(false);
    const [formData, setFormData] = useState({
        name: ''
    });

    const handleCreateCert = async () => {
        const payload = {
            name: formData.name,
        };
        await apiService.createCACert(payload)
            .then(data => {
                onCertCreated(data);
                setShowModal(false);
            })
            .catch(error => {
                console.error('There was a problem creating the new certificate:', error);
            });
    }

    const handleChange = (event) => {
        setFormData({
            ...formData,
            [event.target.name]: event.target.value
        });
    };

    const handleShowModal = () => {
        setShowModal(true);
        setFormData({name: ''});
    };


    return (
        <div>
            <button className="button is-success is-small" onClick={handleShowModal}>Create New Certificate
            </button>
            <div className={`modal ${showModal ? 'is-active' : ''}`}>
                <div className="modal-background"></div>
                <div className="modal-content">

                    <header className="modal-card-head">
                        <p className="modal-card-title">Create New Certificate</p>
                        <button className="delete" aria-label="close" onClick={() => setShowModal(false)}></button>
                    </header>


                    <section className="modal-card-body">
                        <form onSubmit={handleCreateCert}>
                            <label className="label">Certificate Name</label>
                            <input type="text" name="name" className="input is-small" onChange={handleChange}
                                   value={formData.name} placeholder="Name" required/>
                        </form>
                    </section>


                    <footer className="modal-card-foot">
                        <button className="button is-success is-small" type="submit" onClick={handleCreateCert}>Create
                        </button>
                        <button className="button is-small" type="button" onClick={() => setShowModal(false)}>Cancel
                        </button>
                    </footer>

                </div>
            </div>
        </div>
    );
};

export default CreateCACertModal;
