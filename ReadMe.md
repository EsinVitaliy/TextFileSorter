## How to use:

Build all applications and generate text file with **TextFileSorter.Generator** application, then sort text file with **TextFileSorter** application.


## TextFileSorter.Generator parameters:

**-o** Path to output file

**-s** Size for output file, in megabytes

### Example:
```
TextFileSorter.Generator.exe -o text.txt -s 123
```


## TextFileSorter parameters:

**-i** Path to input file

**-o** Path to output sorted file, if omitted the input file will be overwritten

**-m** Maximum chunk size in megabytes, if omitted it will be chosen automatically


### Example:
```
TextFileSorter.exe -i text.txt -o text_sorted.txt -m 50
```
