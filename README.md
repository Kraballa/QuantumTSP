# QuantumTSP
Travelling Salesman on a Quantum Computer.

Developed by Stephen Jordan at https://github.com/stephenjordan/qaoa_tsp. This repo just updates syntax so it compiles with the modern QDK out of the box.

This project runs the Travelling Salesman Optimization Problem (TSP-OPT) using the .net6 framework and qdk on 4 cities with 6 total routes.

See [this blogpost](https://www.avanade.com/nl-nl/blogs/be-orange/technology/quantum-computing-an-optimization-example) for a surface level explanation.

See [the original article](https://quantumalgorithmzoo.org/traveling_santa/) for a more detailed rundown of the maths to form the Ising-Hamiltonian.

# Compile and run
Requires the [Microsoft QDK](https://docs.microsoft.com/en-us/azure/quantum/install-command-line-qdk#prerequisite).
- compile: `dotnet build`
- run: `dotnet run --project TSP`
