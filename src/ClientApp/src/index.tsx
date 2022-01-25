import { ThemeProvider } from "@mui/styles";
import React from 'react';
import ReactDOM from 'react-dom';
import App from './App';

import { theme } from './themes/default';

ReactDOM.render(
  <React.StrictMode>
    <ThemeProvider theme={theme}>
      <App />
    </ThemeProvider>
  </React.StrictMode>,
  document.getElementById('root')
);