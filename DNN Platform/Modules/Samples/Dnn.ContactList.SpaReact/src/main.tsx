import React from 'react';
import ReactDOM from 'react-dom/client';
import App from './App';
const logger = console;
import { SecurityContext } from './types/Security';
import { ModuleContext } from './types/Module';

// Parse context data from the root element
function initializeApp() {
logger.debug('>>> Entering initializeApp()');
  const rootElement = document.querySelector('.contact-list-spa-react');

logger.debug(`initializeApp(): rootElement → ${rootElement}`);
  if (!rootElement) {
    console.error('Root element .contact-list-spa-react not found');
    return;
  }

  // Parse security context
  const securityData = rootElement.getAttribute('data-security');
logger.debug(`initializeApp(): securityData → ${securityData}`);
  const moduleData = rootElement.getAttribute('data-module');
logger.debug(`initializeApp(): moduleData → ${moduleData}`);

  if (!securityData || !moduleData) {
    console.error('Missing data-security or data-module attributes');
    return;
  }

  let security: SecurityContext;
  let moduleContext: ModuleContext;

  try {
    security = JSON.parse(securityData);
    moduleContext = JSON.parse(moduleData);
  } catch (err) {
    console.error('Failed to parse context data:', err);
    return;
  }

  // Create React root and render
  const root = ReactDOM.createRoot(rootElement);
  root.render(
    <React.StrictMode>
      <App security={security} moduleContext={moduleContext} />
    </React.StrictMode>
  );
logger.debug('<<< Exiting initializeApp()');
}

// Wait for DOM to be ready
if (document.readyState === 'loading') {
  document.addEventListener('DOMContentLoaded', initializeApp);
} else {
  initializeApp();
}

