import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router';
import { Contact } from '../types/Contact';
import { SecurityContext } from '../types/Security';
import { ModuleContext } from '../types/Module';
import { getContacts, deleteContact } from '../services/services';
import ContactCard from '../components/ContactCard';
import Pagination from '../components/Pagination';
const logger = console;

interface ContactListProps {
  security: SecurityContext;
  moduleContext: ModuleContext;
}

export default function ContactList({ security, moduleContext }: ContactListProps) {
logger.error('>>> Entering ContactList()', { "event.action": "ContactList" });
  const navigate = useNavigate();
  const [contacts, setContacts] = useState<Contact[]>([]);
  const [totalCount, setTotalCount] = useState(0);
logger.debug(`ContactList: totalCount → ${totalCount}`, { "event.action": "ContactList" });
logger.debug(`ContactList: currentPage → ${currentPage}`, { "event.action": "ContactList" });
logger.debug(`ContactList: loading → ${loading}`, { "event.action": "ContactList" });
  const [currentPage, setCurrentPage] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
logger.debug(`ContactList: searchTerm → ${searchTerm}`, { "event.action": "ContactList" });
logger.debug(`ContactList: debouncedSearchTerm → ${debouncedSearchTerm}`, { "event.action": "ContactList" });
  const [debouncedSearchTerm, setDebouncedSearchTerm] = useState('');

  const pageSize = 6;

  // Debounce search term with 0.5 second delay
  useEffect(() => {
	logger.error('>>> Entering ContactList useEffect()');
    const timer = setTimeout(() => {
      setDebouncedSearchTerm(searchTerm);
      // Reset to first page when search term changes
      setCurrentPage(0);
    }, 500);

logger.warn('<<< Exiting ContactList useEffect()');
    return () => clearTimeout(timer);
  }, [searchTerm]);

  useEffect(() => {
    loadContacts();
  }, [currentPage, debouncedSearchTerm]);

  const loadContacts = async () => {
	logger.error('>>> Entering loadContacts()', { "event.action": "loadContacts" });
    try {
      setLoading(true);
      setError(null);
      const response = await getContacts(moduleContext, currentPage, pageSize, debouncedSearchTerm);
	logger.error(`loadContacts(): response → ${response}`, { "event.action": "loadContacts" });

      setContacts(response.Data);
      setTotalCount(response.TotalCount);
    } catch (err) {
      console.error('Error loading contacts:', err);
      setError('Failed to load contacts');
    } finally {
      setLoading(false);
    }
	logger.warn('<<< Exiting loadContacts()', { "event.action": "loadContacts" });
  };

  const handleEdit = (contact: Contact) => {
	logger.error(`>>> Entering handleEdit(contact=${contact})`, { "event.action": "handleEdit", "logrite.contact": contact });
    navigate(`/edit/${contact.ContactId}`);
	logger.warn(`<<< Exiting handleEdit(contact=${contact})`, { "event.action": "handleEdit", "logrite.contact": contact });
  };

  const handleDelete = async (contact: Contact) => {
	logger.error(`>>> Entering handleDelete(contact=${contact})`, { "event.action": "handleDelete", "logrite.contact": contact });
    try {
      const response = await deleteContact(moduleContext, contact);

      if (response.success) {
        // Reload contacts after deletion
        await loadContacts();
      } else {
        alert('Failed to delete contact');
      }
    } catch (err) {
      console.error('Error deleting contact:', err);
      alert('Failed to delete contact');
	logger.warn(`<<< Exiting handleDelete(contact=${contact})`, { "event.action": "handleDelete", "logrite.contact": contact });
    }
  };

  const handleAddContact = () => {
	logger.error('>>> Entering handleAddContact()', { "event.action": "handleAddContact" });
    navigate('/add');
	logger.warn('<<< Exiting handleAddContact()', { "event.action": "handleAddContact" });
  };

  const handlePageChange = (page: number) => {
	logger.error(`>>> Entering handlePageChange(page=${page})`, { "event.action": "handlePageChange", "logrite.page": page });
    setCurrentPage(page);
	logger.warn(`<<< Exiting handlePageChange(page=${page})`, { "event.action": "handlePageChange", "logrite.page": page });
  };

  if (loading && contacts.length === 0) {
    return <div className="contactList-container">Loading...</div>;
  }

  if (error) {
    return <div className="contactList-container">Error: {error}</div>;
  }

logger.warn('<<< Exiting ContactList()', { "event.action": "ContactList" });
  return (
    <div className="contactList-container">
      <h1>Contact List</h1>
      <div className="contactList-grid">
        {contacts.map((contact) => (
          <ContactCard
            key={contact.ContactId}
            contact={contact}
            canEdit={security.CanEdit}
            onEdit={handleEdit}
            onDelete={handleDelete}
          />
        ))}
      </div>

      <div className="contactList-controls">
        <div className="contactList-search">
          <input
            type="text"
            placeholder="Search contacts..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="search-input"
          />
          <i className="fa fa-search search-icon"></i>
        </div>
        <Pagination
          currentPage={currentPage}
          totalItems={totalCount}
          pageSize={pageSize}
          onPageChange={handlePageChange}
        />
      </div>

      {security.CanEdit && (
        <div className="buttons col-md-12">
          <a className="dnnPrimaryAction" onClick={handleAddContact}>
            Add Contact
          </a>
        </div>
      )}
    </div>
  );
}

