# Detailed UI Rules for Admin Screens and Flows in Electronic Library System

This document outlines detailed UI rules, screens, and navigation flows for the Admin role in the **Electronic Library System**. The UI is designed to be modern, intuitive, and efficient, focusing on dashboard-style management with responsive design. All screens adhere to the following general UI rules:

## General UI Rules
- **Design Style**: Modern and youthful, using a vibrant color palette (primary: #FF6B00 orange, accent: #FFDAB9 peach, neutral: white/black/gray). Sans-serif fonts (e.g., Poppins). Rounded corners (12px), subtle shadows, and gradients for depth.
- **Responsive Design**: Mobile-first; use flexbox/grid for layouts. Breakpoints: mobile (<768px), tablet (768-1024px), desktop (>1024px).
- **Accessibility**: ARIA labels for interactive elements, alt text for images, keyboard navigation, high contrast (WCAG 2.1 compliant).
- **Components**: Reusable elements like buttons (orange primary), tables (sortable/paginated), forms (validation with red errors), modals (for confirmations), charts (using Chart.js for reports).
- **Navigation**: Sidebar menu on desktop, hamburger menu on mobile. Breadcrumbs for deep pages.
- **Error Handling**: Inline errors for forms, toast notifications for actions (success/green, error/red).
- **Performance**: Lazy loading for images/lists, debounce for search inputs.
- **Security**: Role-based hiding of elements (Admin-only views).

## Admin Screens Overview
Admin has access to management-focused screens. Key screens include:
1. **Login/Register**: Shared with User, but redirects to Admin Dashboard after login.
2. **Dashboard**: Overview of system stats.
3. **User Management**: List, view, edit users.
4. **Book Management**: Add, edit, delete books.
5. **Category Management**: Manage categories.
6. **Author Management**: Manage authors.
7. **Borrow Management**: View and manage borrows.
8. **Reports**: Generate analytics.
9. **Profile/Settings**: Admin profile and logout.

## Navigation Flows
Admin flows start from login and center around the Dashboard. Use the following Mermaid diagram for visual representation:

```mermaid
graph TD
    A[Login] -->|Successful Admin Login| B[Dashboard]
    B --> C[User Management]
    B --> D[Book Management]
    B --> E[Category Management]
    B --> F[Author Management]
    B --> G[Borrow Management]
    B --> H[Reports]
    B --> I[Profile/Settings]
    C -->|View/Edit User| J[User Details]
    J -->|Save/Cancel| C
    D -->|Add/Edit Book| K[Book Details/Form]
    K -->|Save/Cancel| D
    E -->|Add/Edit Category| L[Category Form]
    L -->|Save/Cancel| E
    F -->|Add/Edit Author| M[Author Form]
    M -->|Save/Cancel| F
    G -->|View Borrow| N[Borrow Details]
    N -->|Return/Overdue Action| G
    H -->|Generate Report| H  // Self-loop for filters
    I -->|Logout| A
    subgraph "Common Actions"
        O[Search/Filter] --> All Screens
        P[Pagination/Sort] --> List Screens (C,D,E,F,G)
    end
```

### Flow Descriptions
- **From Login to Dashboard**: After authentication (JWT), redirect to Dashboard if role="Admin". Show welcome message with quick stats.
- **Dashboard to Management Screens**: Sidebar links; each click loads the respective list view with search bar.
- **List to Details/Form**: Click row/item opens modal or new page for edit/add. Cancel returns to list; save updates and refreshes list.
- **Reports Flow**: Apply filters (date range, category) and generate charts/tables; export option (CSV/PDF).
- **Logout**: From Profile, clears session and redirects to Login.
- **Error Flows**: Unauthorized access redirects to Login; validation errors prevent submission.

## Detailed Screen Rules
- **Dashboard**:
  - UI Elements: Cards for stats (users count, books count, active borrows, overdue). Charts for borrow trends. Quick links to other screens.
  - Rules: Auto-refresh stats every 5min. Responsive: Cards stack on mobile.

- **User Management**:
  - UI Elements: Table (columns: ID, Username, Email, Role, Active). Search by username/email, filter by role/active. Add/Edit buttons open forms.
  - Rules: Pagination (10/25/50 per page). Confirm modal for delete.

- **Book Management**:
  - UI Elements: Table (Title, Authors, Categories, Year, Available). File upload in add/edit form. Search full-text.
  - Rules: Validate file (PDF/EPUB, <50MB). Preview image if available.

- **Category/Author Management**:
  - UI Elements: Simple list/table (Name, Description/Bio). Add/Edit forms.
  - Rules: Unique name validation. Delete only if not linked to books (confirm modal).

- **Borrow Management**:
  - UI Elements: Table (User, Book, Borrow Date, Due Date, Status). Filter by status/overdue.
  - Rules: Admin can force return or mark overdue.

- **Reports**:
  - UI Elements: Dropdowns for filters, generate button. Display charts (bar/pie for borrows by category) and tables.
  - Rules: Use aggregation queries; export functionality.

- **Profile/Settings**:
  - UI Elements: Form for change password, view details. Logout button.
  - Rules: Secure password change with confirmation.

Adhere to these rules for consistent Admin UI.

Generated on: September 29, 2025