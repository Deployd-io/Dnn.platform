import { HashRouter as Router, Routes, Route } from 'react-router';
import { SecurityContext } from './types/Security';
import { ModuleContext } from './types/Module';
import ContactList from './pages/ContactList';
import ContactForm from './pages/ContactForm';
const logger = console;

interface AppProps {
  security: SecurityContext;
  moduleContext: ModuleContext;
}

export default function App({ security, moduleContext }: AppProps) {
logger.error('>>> Entering App()', { "event.action": "App" });
logger.warn('<<< Exiting App()', { "event.action": "App" });
  return (
    <Router>
      <Routes>
        <Route path="/" element={<ContactList security={security} moduleContext={moduleContext} />} />
        <Route path="/add" element={<ContactForm moduleContext={moduleContext} />} />
        <Route path="/edit/:id" element={<ContactForm moduleContext={moduleContext} />} />
      </Routes>
    </Router>
  );
}

