import React, { useEffect, useRef, useId } from 'react';
import { AlertTriangle, X } from 'lucide-react';
import './MutationUi.css';

interface ConfirmModalProps {
  isOpen: boolean;
  title: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
  confirmVariant?: 'primary' | 'danger';
  submittingText?: string;
  isSubmitting?: boolean;
  error?: string | null;
  /** Lucide icon node to show in the confirm button (optional) */
  confirmIcon?: React.ReactNode;
  onConfirm: () => void;
  onCancel: () => void;
}

export const ConfirmModal: React.FC<ConfirmModalProps> = ({
  isOpen,
  title,
  message,
  confirmText = 'Eliminar',
  cancelText = 'Cancelar',
  confirmVariant = 'danger',
  submittingText = 'Eliminando...',
  isSubmitting = false,
  error = null,
  confirmIcon,
  onConfirm,
  onCancel
}) => {
  const titleId = useId();
  const dialogRef = useRef<HTMLDivElement>(null);
  const cancelBtnRef = useRef<HTMLButtonElement | null>(null);
  const previousFocusRef = useRef<HTMLElement | null>(null);
  // Save focus when opening, restore when closing
  useEffect(() => {
    if (isOpen) {
      previousFocusRef.current = document.activeElement as HTMLElement;
      // Move focus to Cancel button on open (not the destructive action)
      requestAnimationFrame(() => {
        cancelBtnRef.current?.focus();
      });
    } else {
      previousFocusRef.current?.focus();
      previousFocusRef.current = null;
    }
  }, [isOpen]);

  // Handle Escape and Tab keys
  useEffect(() => {
    if (!isOpen) return;

    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape') {
        if (!isSubmitting) {
          onCancel();
        }
        return;
      }

      if (e.key === 'Tab') {
        const focusableNodes = dialogRef.current?.querySelectorAll(
          'a[href], button:not([disabled]), input:not([disabled]), select:not([disabled]), textarea:not([disabled]), [tabindex]:not([tabindex="-1"])'
        );

        let focusableElements: HTMLElement[] = [];
        if (focusableNodes) {
          focusableElements = Array.from(focusableNodes) as HTMLElement[];
          // Optionally exclude hidden elements
          // focusableElements = focusableElements.filter(el => el.offsetParent !== null);
        }

        if (focusableElements.length === 0) {
          e.preventDefault();
          dialogRef.current?.focus();
          return;
        }

        const firstElement = focusableElements[0];
        const lastElement = focusableElements[focusableElements.length - 1];

        const activeElement = document.activeElement as HTMLElement;
        const isOutside = !dialogRef.current?.contains(activeElement);

        if (isOutside) {
          e.preventDefault();
          if (e.shiftKey) {
            lastElement.focus();
          } else {
            firstElement.focus();
          }
          return;
        }

        if (e.shiftKey) {
          if (activeElement === firstElement) {
            lastElement.focus();
            e.preventDefault();
          }
        } else {
          if (activeElement === lastElement) {
            firstElement.focus();
            e.preventDefault();
          }
        }
      }
    };

    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [isOpen, isSubmitting, onCancel]);

  if (!isOpen) return null;

  return (
    <div
      className="mutation-modal-overlay"
      onClick={() => {
        if (!isSubmitting) onCancel();
      }}
    >
      <div
        ref={dialogRef}
        className="mutation-modal-dialog"
        role="dialog"
        aria-modal="true"
        tabIndex={-1}
        aria-labelledby={titleId}
        onClick={(e) => e.stopPropagation()}
      >
        <div className="mutation-modal-header">
          <h3 id={titleId} className="mutation-modal-title">
            {title}
          </h3>
          <button
            type="button"
            className="mutation-modal-close-btn"
            aria-label="Cerrar modal"
            onClick={onCancel}
            disabled={isSubmitting}
          >
            <X size={18} aria-hidden="true" />
          </button>
        </div>

        <div className="mutation-modal-body">
          {error && (
            <div
              className="mutation-error-banner"
              role="alert"
              aria-live="assertive"
            >
              <AlertTriangle size={15} aria-hidden="true" style={{ flexShrink: 0 }} />
              <span>{error}</span>
            </div>
          )}
          <p className="mutation-confirm-message">
            {message}
          </p>
        </div>

        <div className="mutation-modal-footer">
          <button
            ref={cancelBtnRef}
            type="button"
            className="mutation-btn mutation-btn-secondary"
            onClick={onCancel}
            disabled={isSubmitting}
          >
            {cancelText}
          </button>
          <button
            type="button"
            className={`mutation-btn mutation-btn-${confirmVariant}`}
            onClick={onConfirm}
            disabled={isSubmitting}
          >
            {isSubmitting ? (
              <>
                <span className="mutation-spinner" aria-hidden="true"></span>
                <span>{submittingText}</span>
              </>
            ) : (
              <>
                {confirmIcon && <span aria-hidden="true">{confirmIcon}</span>}
                {confirmText}
              </>
            )}
          </button>
        </div>
      </div>
    </div>
  );
};
