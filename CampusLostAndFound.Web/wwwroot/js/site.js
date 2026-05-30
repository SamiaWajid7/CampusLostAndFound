// Campus Lost and Found - Site JavaScript

// DOM Ready
document.addEventListener('DOMContentLoaded', function() {
    initializeTooltips();
    initializeFileUpload();
    initializeNotifications();
    initializeImagePreview();
});

// Initialize Bootstrap tooltips
function initializeTooltips() {
    const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
    tooltipTriggerList.forEach(function(tooltipTriggerEl) {
        new bootstrap.Tooltip(tooltipTriggerEl);
    });
}

// File Upload with Drag and Drop
function initializeFileUpload() {
    const uploadAreas = document.querySelectorAll('.file-upload-area');
    
    uploadAreas.forEach(function(area) {
        const fileInput = area.querySelector('input[type="file"]');
        
        if (!fileInput) return;
        
        // Click to upload
        area.addEventListener('click', function(e) {
            if (e.target !== fileInput) {
                fileInput.click();
            }
        });
        
        // Drag events
        area.addEventListener('dragover', function(e) {
            e.preventDefault();
            area.classList.add('dragover');
        });
        
        area.addEventListener('dragleave', function(e) {
            e.preventDefault();
            area.classList.remove('dragover');
        });
        
        area.addEventListener('drop', function(e) {
            e.preventDefault();
            area.classList.remove('dragover');
            
            const files = e.dataTransfer.files;
            if (files.length > 0) {
                fileInput.files = files;
                handleFileSelect(fileInput, area);
            }
        });
        
        // File selected
        fileInput.addEventListener('change', function() {
            handleFileSelect(fileInput, area);
        });
    });
}

// Handle file selection
function handleFileSelect(input, area) {
    const previewContainer = area.querySelector('.preview-container') || createPreviewContainer(area);
    previewContainer.innerHTML = '';
    
    Array.from(input.files).forEach(function(file) {
        if (file.type.startsWith('image/')) {
            const reader = new FileReader();
            reader.onload = function(e) {
                const img = document.createElement('img');
                img.src = e.target.result;
                img.className = 'img-thumbnail m-1';
                img.style.maxWidth = '100px';
                img.style.maxHeight = '100px';
                previewContainer.appendChild(img);
            };
            reader.readAsDataURL(file);
        } else {
            const fileInfo = document.createElement('div');
            fileInfo.className = 'badge bg-secondary m-1';
            fileInfo.textContent = file.name;
            previewContainer.appendChild(fileInfo);
        }
    });
}

// Create preview container
function createPreviewContainer(area) {
    const container = document.createElement('div');
    container.className = 'preview-container mt-2 d-flex flex-wrap';
    area.appendChild(container);
    return container;
}

// Notifications polling
function initializeNotifications() {
    const notificationBell = document.querySelector('.notification-bell');
    if (!notificationBell) return;
    
    // Poll for new notifications every 30 seconds
    setInterval(function() {
        fetchNotificationCount();
    }, 30000);
}

// Fetch notification count
function fetchNotificationCount() {
    fetch('/api/notifications/unread-count')
        .then(response => response.json())
        .then(data => {
            const badge = document.querySelector('.notification-badge');
            if (badge) {
                if (data.count > 0) {
                    badge.textContent = data.count > 99 ? '99+' : data.count;
                    badge.style.display = 'flex';
                } else {
                    badge.style.display = 'none';
                }
            }
        })
        .catch(error => console.error('Error fetching notifications:', error));
}

// Mark notification as read
function markAsRead(notificationId) {
    fetch(`/api/notifications/${notificationId}/read`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        }
    })
    .then(response => {
        if (response.ok) {
            const item = document.querySelector(`[data-notification-id="${notificationId}"]`);
            if (item) {
                item.classList.remove('unread');
            }
            fetchNotificationCount();
        }
    })
    .catch(error => console.error('Error marking notification as read:', error));
}

// Image preview modal
function initializeImagePreview() {
    const images = document.querySelectorAll('.image-gallery img, .item-image');
    
    images.forEach(function(img) {
        img.addEventListener('click', function() {
            showImageModal(this.src, this.alt);
        });
    });
}

// Show image in modal
function showImageModal(src, alt) {
    let modal = document.getElementById('imagePreviewModal');
    
    if (!modal) {
        modal = document.createElement('div');
        modal.id = 'imagePreviewModal';
        modal.className = 'modal fade';
        modal.innerHTML = `
            <div class="modal-dialog modal-lg modal-dialog-centered">
                <div class="modal-content">
                    <div class="modal-header">
                        <h5 class="modal-title">${alt || 'Image Preview'}</h5>
                        <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body text-center">
                        <img src="${src}" class="img-fluid" alt="${alt}">
                    </div>
                </div>
            </div>
        `;
        document.body.appendChild(modal);
    } else {
        modal.querySelector('img').src = src;
        modal.querySelector('.modal-title').textContent = alt || 'Image Preview';
    }
    
    const bsModal = new bootstrap.Modal(modal);
    bsModal.show();
}

// Confirm delete
function confirmDelete(message, formId) {
    if (confirm(message || 'Are you sure you want to delete this item?')) {
        document.getElementById(formId).submit();
    }
}

// Toast notification
function showToast(message, type = 'info') {
    let container = document.querySelector('.toast-container');
    
    if (!container) {
        container = document.createElement('div');
        container.className = 'toast-container';
        document.body.appendChild(container);
    }
    
    const toastId = 'toast-' + Date.now();
    const bgClass = {
        'success': 'bg-success',
        'error': 'bg-danger',
        'warning': 'bg-warning',
        'info': 'bg-info'
    }[type] || 'bg-info';
    
    const toastHtml = `
        <div id="${toastId}" class="toast" role="alert">
            <div class="toast-header ${bgClass} text-white">
                <strong class="me-auto">Notification</strong>
                <button type="button" class="btn-close btn-close-white" data-bs-dismiss="toast"></button>
            </div>
            <div class="toast-body">
                ${message}
            </div>
        </div>
    `;
    
    container.insertAdjacentHTML('beforeend', toastHtml);
    
    const toastEl = document.getElementById(toastId);
    const toast = new bootstrap.Toast(toastEl, { autohide: true, delay: 5000 });
    toast.show();
    
    toastEl.addEventListener('hidden.bs.toast', function() {
        toastEl.remove();
    });
}

// Form validation
function validateForm(formId) {
    const form = document.getElementById(formId);
    if (!form) return true;
    
    let isValid = true;
    const requiredFields = form.querySelectorAll('[required]');
    
    requiredFields.forEach(function(field) {
        if (!field.value.trim()) {
            field.classList.add('is-invalid');
            isValid = false;
        } else {
            field.classList.remove('is-invalid');
        }
    });
    
    return isValid;
}

// Search with debounce
let searchTimeout;
function debounceSearch(input, callback, delay = 300) {
    clearTimeout(searchTimeout);
    searchTimeout = setTimeout(function() {
        callback(input.value);
    }, delay);
}

// Filter items
function filterItems() {
    const form = document.getElementById('filterForm');
    if (form) {
        form.submit();
    }
}

// Export for use in other scripts
window.CampusLostFound = {
    showToast: showToast,
    confirmDelete: confirmDelete,
    validateForm: validateForm,
    markAsRead: markAsRead
};
