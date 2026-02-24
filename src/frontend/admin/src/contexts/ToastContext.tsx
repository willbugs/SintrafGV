import React, { createContext, useContext, ReactNode } from 'react';
import { useSnackbar } from 'notistack';

type SnackbarVariant = 'default' | 'error' | 'success' | 'warning' | 'info';

interface ToastContextData {
  success: (title: string, message: string) => void;
  error: (title: string, message: string) => void;
  warning: (title: string, message: string) => void;
  info: (title: string, message: string) => void;
}

const ToastContext = createContext<ToastContextData>({} as ToastContextData);

interface ToastProviderProps {
  children: ReactNode;
}

export const ToastProvider: React.FC<ToastProviderProps> = ({ children }) => {
  const { enqueueSnackbar } = useSnackbar();

  const showToast = (title: string, message: string, variant: SnackbarVariant) => {
    enqueueSnackbar(
      <div>
        <strong>{title}</strong>
        <br />
        {message}
      </div>,
      {
        variant,
        autoHideDuration: 5000,
        anchorOrigin: {
          vertical: 'top',
          horizontal: 'right',
        },
      }
    );
  };

  const success = (title: string, message: string) => {
    showToast(title, message, 'success');
  };

  const error = (title: string, message: string) => {
    showToast(title, message, 'error');
  };

  const warning = (title: string, message: string) => {
    showToast(title, message, 'warning');
  };

  const info = (title: string, message: string) => {
    showToast(title, message, 'info');
  };

  return (
    <ToastContext.Provider value={{ success, error, warning, info }}>
      {children}
    </ToastContext.Provider>
  );
};

export const useToast = (): ToastContextData => {
  const context = useContext(ToastContext);
  if (!context) {
    throw new Error('useToast deve ser usado dentro de ToastProvider');
  }
  return context;
};
