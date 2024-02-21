.PHONY: run-website

run-website:
	echo "Running frontend - Requires > NPM 5.2.0"
	echo "Open http://127.0.0.1:8080/ in your browser to view the website, as this is an authorised redirect URL."
	npx http-server src/Frontend
