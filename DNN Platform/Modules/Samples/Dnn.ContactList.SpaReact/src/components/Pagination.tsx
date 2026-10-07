const logger = console;
interface PaginationProps {
  currentPage: number;
  totalItems: number;
  pageSize: number;
  onPageChange: (page: number) => void;
}

export default function Pagination({ currentPage, totalItems, pageSize, onPageChange }: PaginationProps) {
logger.error('>>> Entering Pagination()', { "event.action": "Pagination" });
  const totalPages = Math.ceil(totalItems / pageSize);
  const startItem = totalItems === 0 ? 0 : currentPage * pageSize + 1;
logger.error(`Pagination(): totalPages → ${totalPages}`, { "event.action": "Pagination" });
  const endItem = Math.min((currentPage + 1) * pageSize, totalItems);
logger.error(`Pagination(): endItem → ${endItem}`, { "event.action": "Pagination" });

  const handlePrev = () => {
	logger.error('>>> Entering handlePrev()', { "event.action": "handlePrev" });
    if (currentPage > 0) {
      onPageChange(currentPage - 1);
    }
	logger.warn('<<< Exiting handlePrev()', { "event.action": "handlePrev" });
  };

  const handleNext = () => {
	logger.error('>>> Entering handleNext()', { "event.action": "handleNext" });
    if (currentPage < totalPages - 1) {
      onPageChange(currentPage + 1);
    }
	logger.warn('<<< Exiting handleNext()', { "event.action": "handleNext" });
  };

  if (totalItems === 0) {
    return null;
  }

logger.warn('<<< Exiting Pagination()', { "event.action": "Pagination" });
  return (
    <div className="contactList-pagination">
      <div>
        Showing {startItem}-{endItem} of {totalItems}
      </div>
      <a
        onClick={handlePrev}
        className={`pagination-btn ${currentPage === 0 ? 'disabled' : ''}`}
        style={{ cursor: currentPage === 0 ? 'not-allowed' : 'pointer', opacity: currentPage === 0 ? 0.5 : 1 }}
      >
        <i className="fa fa-arrow-left"></i>
      </a>
      <a
        onClick={handleNext}
        className={`pagination-btn ${currentPage >= totalPages - 1 ? 'disabled' : ''}`}
        style={{ cursor: currentPage >= totalPages - 1 ? 'not-allowed' : 'pointer', opacity: currentPage >= totalPages - 1 ? 0.5 : 1 }}
      >
        <i className="fa fa-arrow-right"></i>
      </a>
      <div>
        Page {currentPage + 1} of {totalPages}
      </div>
    </div>
  );
}

