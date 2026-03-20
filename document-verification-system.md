# Document Verification System — Architecture

## Overview
A complete KYC document verification system for the Smart Insurance Hub. Customers and Agents upload required documents, Admin reviews and approves/rejects. Verified users can purchase policies.

## Architecture

```
User Upload → File Validation (type, size)
            → Cloudflare R2 (file storage)
            → PostgreSQL/Supabase (metadata: user_id, category, status, URL)
            → Admin Review Panel
            → Approve / Reject
            → Auto-update verification_status
```

## File Structure

| File | Purpose |
|------|---------|
| `Models/UserDocument.cs` | Document metadata entity |
| `Services/R2StorageService.cs` | Cloudflare R2 upload/download/delete + local fallback |
| `Services/DocumentService.cs` | Required docs definitions, progress tracking, verification logic |
| `Controllers/DocumentController.cs` | Profile page, upload, delete, preview |
| `Controllers/AdminController.cs` | Document verification panel (approve/reject) |
| `Views/Document/Profile.cshtml` | User profile with Documents tab |
| `Views/Admin/DocumentVerification.cshtml` | Admin review table with modals |

## Required Documents

### Customer
1. Identity Proof (Aadhaar, PAN)
2. Address Proof (Aadhaar, Electricity Bill)
3. Age Proof (Birth Certificate, Passport)
4. Income Proof (Salary Slip, ITR, Bank Statement)
5. Photograph (Passport Size Photo)
6. Bank Details (Cancelled Cheque, Bank Passbook)
7. Medical Documents (Medical Report)

### Agent
1. Identity Proof (Aadhaar, PAN)
2. Address Proof (Aadhaar, Electricity Bill)
3. Professional License (IRDAI Agent License)
4. Education (SSC/HSC, Graduation Certificate)
5. Photograph (Passport Size Photo)
6. Bank Details (Cancelled Cheque, Bank Passbook)
7. Company Authorization (Appointment Letter)

## Verification Flow

1. User uploads a document → status = `pending`
2. Admin sees it in Document Verification panel
3. Admin clicks **Approve** → status = `approved`
4. After all 7 categories have at least one approved doc → `verification_status = "verified"`
5. Admin clicks **Reject** → status = `rejected` with reason → user can re-upload

## Security
- File validation: PDF, JPG, PNG only, max 10MB
- Presigned URLs for file access (1-hour expiry)
- Authenticated uploads only
- No public write access to R2 bucket
