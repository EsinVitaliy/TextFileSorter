## How to use:

Generate file with **TextFileSorter.Generator** application, then sort file with **TextFileSorter** application.


## Generator parameters:

\-o Path to output file

\-s Size for output file, in megabytes

Example:
```
TextFileSorter.Generator.exe -o c:\\Test\\text.txt -s 123
```


## Main application parameters:

\-i Path to input file

\-o Path to output sorted file, if omitted the input file will be overwritten

\-m Maximum chunk size in megabytes, if omitted it will be chosen automatically


Example:
```
TextFileSorter.exe -i c:\\Test\\text.txt -o c:\\Test\\text\_sorted.txt -m 50
```
