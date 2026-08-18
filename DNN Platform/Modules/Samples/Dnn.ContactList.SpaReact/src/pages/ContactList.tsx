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
logger.debug(`>>> Entering ContactList(security=${security},moduleContext=${moduleContext})`);
  const navigate = useNavigate();
  const [contacts, setContacts] = useState<Contact[]>([]);
  const [totalCount, setTotalCount] = useState(0);
  const [currentPage, setCurrentPage] = useState(0);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [searchTerm, setSearchTerm] = useState('');
  const [debouncedSearchTerm, setDebouncedSearchTerm] = useState('');

  const pageSize = 6;

  // Debounce search term with 0.5 second delay
  useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedSearchTerm(searchTerm);
      // Reset to first page when search term changes
      setCurrentPage(0);
    }, 500);

    return () => clearTimeout(timer);
  }, [searchTerm]);

  useEffect(() => {
    loadContacts();
  }, [currentPage, debouncedSearchTerm]);

  const loadContacts = async () => {
	logger.debug('>>> Entering loadContacts()');
    try {
      setLoading(true);
      setError(null);
      const response = await getContacts(moduleContext, currentPage, pageSize, debouncedSearchTerm);
	logger.debug(`loadContacts(): response → ${response}`);

      setContacts(response.Data);
      setTotalCount(response.TotalCount);
    } catch (err) {
      console.error('Error loading contacts:', err);
      setError('Failed to load contacts');
    } finally {
      setLoading(false);
    }
	logger.debug('<<< Exiting loadContacts()');
  };

  const handleEdit = (contact: Contact) => {
	logger.debug(`>>> Entering handleEdit(contact=${contact})`);
    navigate(`/edit/${contact.ContactId}`);
	logger.debug(`<<< Exiting handleEdit(contact=${contact})`);
  };

  const handleDelete = async (contact: Contact) => {
	logger.debug(`>>> Entering handleDelete(contact=${contact})`);
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
	logger.debug(`<<< Exiting handleDelete(contact=${contact})`);
    }
  };

  const handleAddContact = () => {
	logger.debug('>>> Entering handleAddContact()');
    navigate('/add');
	logger.debug('<<< Exiting handleAddContact()');
  };

  const handlePageChange = (page: number) => {
	logger.debug(`>>> Entering handlePageChange(page=${page})`);
    setCurrentPage(page);
	logger.debug(`<<< Exiting handlePageChange(page=${page})`);
  };

  if (loading && contacts.length === 0) {
    return <div className="contactList-container">Loading...</div>;
  }

  if (error) {
logger.debug(`<<< Exiting ContactList(security=${security},moduleContext=${moduleContext})`);
    return <div className="contactList-container">Error: {error}</div>;
  }

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

