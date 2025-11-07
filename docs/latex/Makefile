# Makefile for LaTeX document compilation

.PHONY: all clean cleanall view help

# Main target
all: main.pdf

# Compile the main document
main.pdf: main.tex
	pdflatex -interaction=nonstopmode main.tex
	bibtex main
	pdflatex -interaction=nonstopmode main.tex
	pdflatex -interaction=nonstopmode main.tex

# Open the PDF (macOS)
view: main.pdf
	open main.pdf

# Clean auxiliary files
clean:
	rm -f *.aux *.log *.out *.toc *.lof *.lot *.bbl *.blg *.fls *.fdb_latexmk *.synctex.gz
	rm -rf _minted-*

# Clean all generated files including PDF
cleanall:
	rm -f *.aux *.log *.out *.toc *.lof *.lot *.bbl *.blg *.fls *.fdb_latexmk *.synctex.gz *.run.xml
	rm -rf _minted-*
	rm -f main.pdf

# Help message
help:
	@echo "Available targets:"
	@echo "  make all       - Compile the document (default)"
	@echo "  make view      - Compile and open the PDF"
	@echo "  make clean     - Remove auxiliary files"
	@echo "  make cleanall  - Remove all generated files"
	@echo "  make help      - Show this help message"
