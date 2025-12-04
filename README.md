# Intern Time Sheet App

This is a web app for interns and their supervisors used to track interns' time entries.
Upon opening the web app, user will be prompted to log in. There are four pre-made accounts (intern-, intern2-, super-, and admin@email.com) with passwords set in the user secrets file (secrets.json).
Only supervisors and admins can register user accounts, and only admins can create a new admin account. To create a new account, please sign in using the pre-made supervisor or admin accounts, then register your new account using the link in the navbar.

Logging in will land users at the homepage, where interns can use the navigation bar at the top of the screen to view their time entries or create a new one (supervisors cannot create new time entries).
When viewing the list of time entries, users can filter time entries for submitting user, week starting date, and approval status using dropdown menus.
Users may also use the "Generate Report" button to download a PDF report of their hours worked; this can also be filtered using the above dropdowns.
Supervisors and admins can approve time entries using the "Approve" button at the end of each entry's row, or using the "Approve All" button at the top of the page in conjunction with the filtering dropdown menus.

Interns can create new entries by clicking on the "New Time Entry" link on the navbar, or the "Create New" button at the top of the "View Time Entries" page.
When creating & submitting a new entry, specify the week's starting date using the calendar tool. The developer suggests using Sunday to mark week starts, but individual policies may vary. Communicate with your staff when you want the week to start!
Users must specify a "Time In" when creating new entries. To add a "Time Out", users must find their entry on the list in "View Time Entries" and click on "Edit" near the end of the entry's row to amend said entry.
Users cannot submit a time entry for a future date/time, nor can they submit an entry where their "Time Out" is before their "Time In".
